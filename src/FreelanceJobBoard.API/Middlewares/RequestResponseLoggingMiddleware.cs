using Serilog;
using System.Diagnostics;
using System.Text;

namespace FreelanceJobBoard.API.Middlewares;

public class RequestResponseLoggingMiddleware : IMiddleware
{
    private static readonly string[] BodyCaptureMethods = ["POST", "PUT", "PATCH"];
    private const int MaxBodyLength = 4096;
    private const int MaxHeaderValueLength = 512;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip logging for static resources and infrastructure endpoints
        if (ShouldSkipLogging(context.Request.Path))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        var enrichedLogger = Log.ForContext("RequestId", requestId)
                               .ForContext("UserId", context.User?.Identity?.Name ?? "Anonymous")
                               .ForContext("RemoteIP", context.Connection.RemoteIpAddress?.ToString())
                               .ForContext("UserAgent", GetSafeHeaderValue(context.Request.Headers.UserAgent));

        try
        {
            await LogRequestAsync(context, enrichedLogger);

            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await next(context);

            stopwatch.Stop();

            await LogResponseAsync(context, enrichedLogger, stopwatch.ElapsedMilliseconds, responseBodyStream);

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            enrichedLogger.Error(ex, "?? Request processing failed for {Method} {Path} after {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private static async Task LogRequestAsync(HttpContext context, Serilog.ILogger enrichedLogger)
    {
        var request = context.Request;
        var method = request.Method;
        var path = request.Path;
        var queryString = request.QueryString.HasValue ? request.QueryString.Value : "";

        // Log incoming request with color indicators
        var methodEmoji = GetMethodEmoji(method);
        if (string.IsNullOrEmpty(queryString))
        {
            enrichedLogger.Information("?? {MethodEmoji} {Method} {Path}",
                methodEmoji, method, path);
        }
        else
        {
            enrichedLogger.Information("?? {MethodEmoji} {Method} {Path}{QueryString}",
                methodEmoji, method, path, queryString);
        }

        // Log comprehensive request headers
        await LogRequestHeadersAsync(request, enrichedLogger);

        // Capture and log request body for write operations
        if (BodyCaptureMethods.Contains(method.ToUpperInvariant()) && 
            request.ContentLength > 0)
        {
            var requestBody = await ReadRequestBodyAsync(request);
            if (!string.IsNullOrEmpty(requestBody))
            {
                var contentType = request.ContentType ?? "unknown";
                enrichedLogger.Information("?? Request Body [{ContentType}]: {RequestBody}", 
                    contentType, requestBody);
            }
        }
    }

    private static Task LogRequestHeadersAsync(HttpRequest request, Serilog.ILogger enrichedLogger)
    {
        var importantHeaders = new Dictionary<string, string>();
        var securityHeaders = new Dictionary<string, string>();
        var allHeaders = new Dictionary<string, string>();

        foreach (var header in request.Headers)
        {
            var safeValue = GetSafeHeaderValue(header.Value.ToString() ?? "");
            allHeaders[header.Key] = safeValue;

            // Categorize headers
            if (IsImportantHeader(header.Key))
            {
                importantHeaders[header.Key] = safeValue;
            }

            if (IsSecurityHeader(header.Key))
            {
                securityHeaders[header.Key] = IsSensitiveHeader(header.Key) ? "[REDACTED]" : safeValue;
            }
        }

        // Log important headers separately
        if (importantHeaders.Any())
        {
            enrichedLogger.Information("?? Important Headers: {@ImportantHeaders}", importantHeaders);
        }

        // Log security headers
        if (securityHeaders.Any())
        {
            enrichedLogger.Debug("?? Security Headers: {@SecurityHeaders}", securityHeaders);
        }

        // Log all headers at debug level
        enrichedLogger.Debug("?? All Request Headers: {@AllHeaders}", allHeaders);

        // Log specific headers with special formatting
        if (request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                var authType = authHeader.Split(' ').FirstOrDefault() ?? "Unknown";
                enrichedLogger.Information("?? Authorization: {AuthType} [TOKEN_REDACTED]", authType);
            }
        }

        if (request.Headers.ContainsKey("X-Forwarded-For"))
        {
            enrichedLogger.Information("?? X-Forwarded-For: {ForwardedFor}", 
                request.Headers["X-Forwarded-For"].ToString());
        }

        return Task.CompletedTask;
    }

    private static async Task LogResponseAsync(HttpContext context, Serilog.ILogger enrichedLogger, 
        long elapsedMilliseconds, MemoryStream responseBodyStream)
    {
        var response = context.Response;
        var statusCode = response.StatusCode;
        var method = context.Request.Method;
        var path = context.Request.Path;

        var statusCategory = GetStatusCategory(statusCode);
        var statusEmoji = GetStatusEmoji(statusCode);
        var performanceEmoji = GetPerformanceEmoji(elapsedMilliseconds);

        // Log response with color indicators and performance metrics
        if (statusCode >= 400)
        {
            enrichedLogger.Warning("?? {StatusEmoji} {Method} {Path} ? {StatusCode} {StatusCategory} {PerformanceEmoji} {ElapsedMs}ms",
                statusEmoji, method, path, statusCode, statusCategory, performanceEmoji, elapsedMilliseconds);
        }
        else
        {
            enrichedLogger.Information("?? {StatusEmoji} {Method} {Path} ? {StatusCode} {StatusCategory} {PerformanceEmoji} {ElapsedMs}ms",
                statusEmoji, method, path, statusCode, statusCategory, performanceEmoji, elapsedMilliseconds);
        }

        // Log response headers
        await LogResponseHeadersAsync(response, enrichedLogger);

        // Log response body for errors or when specifically needed
        if (statusCode >= 400 || ShouldLogResponseBody(context))
        {
            var responseBody = await ReadResponseBodyAsync(responseBodyStream);
            if (!string.IsNullOrEmpty(responseBody))
            {
                var contentType = response.ContentType ?? "unknown";
                enrichedLogger.Information("?? Response Body [{ContentType}]: {ResponseBody}", 
                    contentType, responseBody);
            }
        }

        // Log additional response details for errors or slow requests
        if (statusCode >= 400 || elapsedMilliseconds > 1000)
        {
            enrichedLogger.Debug("?? Response Metadata: ContentType={ContentType}, ContentLength={ContentLength}, Headers={HeaderCount}",
                response.ContentType ?? "N/A",
                response.ContentLength ?? 0,
                response.Headers.Count);
        }

        // Log performance warnings
        if (elapsedMilliseconds > 5000)
        {
            enrichedLogger.Warning("?? VERY SLOW REQUEST: {Method} {Path} took {ElapsedMs}ms", 
                method, path, elapsedMilliseconds);
        }
        else if (elapsedMilliseconds > 2000)
        {
            enrichedLogger.Warning("?? SLOW REQUEST: {Method} {Path} took {ElapsedMs}ms", 
                method, path, elapsedMilliseconds);
        }
    }

    private static Task LogResponseHeadersAsync(HttpResponse response, Serilog.ILogger enrichedLogger)
    {
        var securityHeaders = new Dictionary<string, string>();
        var cacheHeaders = new Dictionary<string, string>();
        var corsHeaders = new Dictionary<string, string>();

        foreach (var header in response.Headers)
        {
            var safeValue = GetSafeHeaderValue(header.Value.ToString() ?? "");

            if (IsSecurityResponseHeader(header.Key))
            {
                securityHeaders[header.Key] = safeValue;
            }

            if (IsCacheHeader(header.Key))
            {
                cacheHeaders[header.Key] = safeValue;
            }

            if (IsCorsHeader(header.Key))
            {
                corsHeaders[header.Key] = safeValue;
            }
        }

        // Log categorized response headers
        if (securityHeaders.Any())
        {
            enrichedLogger.Debug("??? Security Response Headers: {@SecurityHeaders}", securityHeaders);
        }

        if (cacheHeaders.Any())
        {
            enrichedLogger.Debug("?? Cache Headers: {@CacheHeaders}", cacheHeaders);
        }

        if (corsHeaders.Any())
        {
            enrichedLogger.Debug("?? CORS Headers: {@CorsHeaders}", corsHeaders);
        }

        // Log all response headers at trace level
        var allResponseHeaders = response.Headers.ToDictionary(h => h.Key, h => GetSafeHeaderValue(h.Value.ToString() ?? ""));
        enrichedLogger.Verbose("?? All Response Headers: {@AllResponseHeaders}", allResponseHeaders);

        return Task.CompletedTask;
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (body.Length > MaxBodyLength)
            {
                return body[..MaxBodyLength] + " [TRUNCATED]";
            }

            return body;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to read request body for logging");
            return null;
        }
    }

    private static async Task<string?> ReadResponseBodyAsync(MemoryStream responseBodyStream)
    {
        try
        {
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            if (body.Length > MaxBodyLength)
            {
                return body[..MaxBodyLength] + " [TRUNCATED]";
            }

            return body;
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to read response body for logging");
            return null;
        }
    }

    private static bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant();
        
        return pathValue?.EndsWith(".css") == true ||
               pathValue?.EndsWith(".js") == true ||
               pathValue?.EndsWith(".ico") == true ||
               pathValue?.EndsWith(".png") == true ||
               pathValue?.EndsWith(".jpg") == true ||
               pathValue?.EndsWith(".jpeg") == true ||
               pathValue?.EndsWith(".gif") == true ||
               pathValue?.EndsWith(".svg") == true ||
               pathValue?.EndsWith(".woff") == true ||
               pathValue?.EndsWith(".woff2") == true ||
               pathValue?.EndsWith(".ttf") == true ||
               pathValue?.Contains("/swagger/") == true ||
               pathValue?.Contains("/_framework/") == true ||
               pathValue?.Contains("/health") == true ||
               pathValue?.Contains("/favicon") == true;
    }

    private static bool ShouldLogResponseBody(HttpContext context)
    {
        // Log response body for specific scenarios
        var contentType = context.Response.ContentType?.ToLowerInvariant();
        return contentType?.Contains("application/json") == true ||
               contentType?.Contains("application/xml") == true ||
               contentType?.Contains("text/") == true;
    }

    private static string GetStatusCategory(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "SUCCESS",
            >= 300 and < 400 => "REDIRECT",
            >= 400 and < 500 => "CLIENT_ERROR",
            >= 500 => "SERVER_ERROR",
            _ => "INFORMATIONAL"
        };
    }

    private static string GetStatusEmoji(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "?", // Success
            >= 300 and < 400 => "??", // Redirect
            >= 400 and < 500 => "??",  // Client Error
            >= 500 => "??",           // Server Error
            _ => "??"                 // Info
        };
    }

    private static string GetMethodEmoji(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => "??",
            "POST" => "??",
            "PUT" => "??",
            "PATCH" => "??",
            "DELETE" => "???",
            "HEAD" => "??",
            "OPTIONS" => "??",
            _ => "?"
        };
    }

    private static string GetPerformanceEmoji(long elapsedMilliseconds)
    {
        return elapsedMilliseconds switch
        {
            < 100 => "?", // Very fast
            < 500 => "??", // Fast
            < 1000 => "??", // Normal
            < 2000 => "??", // Slow
            < 5000 => "??", // Very slow
            _ => "??"      // Extremely slow
        };
    }

    private static string GetSafeHeaderValue(string headerValue)
    {
        if (string.IsNullOrEmpty(headerValue))
            return "";

        return headerValue.Length > MaxHeaderValueLength 
            ? headerValue[..MaxHeaderValueLength] + "[TRUNCATED]"
            : headerValue;
    }

    private static bool IsImportantHeader(string headerName)
    {
        var important = new[] 
        {
            "Content-Type", "Content-Length", "Accept", "Accept-Encoding",
            "Cache-Control", "Connection", "Host", "Referer"
        };
        return important.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSecurityHeader(string headerName)
    {
        var security = new[] 
        {
            "Authorization", "Cookie", "X-API-Key", "X-Auth-Token",
            "X-Forwarded-For", "X-Real-IP", "X-Forwarded-Proto"
        };
        return security.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitive = new[] { "Authorization", "Cookie", "X-API-Key", "X-Auth-Token" };
        return sensitive.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsSecurityResponseHeader(string headerName)
    {
        var security = new[] 
        {
            "Set-Cookie", "X-Frame-Options", "X-Content-Type-Options",
            "X-XSS-Protection", "Strict-Transport-Security", "Content-Security-Policy"
        };
        return security.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsCacheHeader(string headerName)
    {
        var cache = new[] { "Cache-Control", "Expires", "ETag", "Last-Modified" };
        return cache.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsCorsHeader(string headerName)
    {
        var cors = new[] 
        {
            "Access-Control-Allow-Origin", "Access-Control-Allow-Methods",
            "Access-Control-Allow-Headers", "Access-Control-Expose-Headers"
        };
        return cors.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }
}