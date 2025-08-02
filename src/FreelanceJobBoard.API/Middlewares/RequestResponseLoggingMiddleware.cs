using Serilog;
using System.Diagnostics;
using System.Text;

namespace FreelanceJobBoard.API.Middlewares;

public class RequestResponseLoggingMiddleware : IMiddleware
{
    private static readonly string[] BodyCaptureMethods = ["POST", "PUT", "PATCH"];
    private const int MaxBodyLength = 2048;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (ShouldSkipLogging(context.Request.Path))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        var enrichedLogger = Log.ForContext("RequestId", requestId)
                               .ForContext("UserId", context.User?.Identity?.Name ?? "Anonymous")
                               .ForContext("RemoteIP", context.Connection.RemoteIpAddress?.ToString());

        try
        {
            await LogRequestAsync(context, enrichedLogger);

            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await next(context);

            stopwatch.Stop();

            LogResponse(context, enrichedLogger, stopwatch.ElapsedMilliseconds);

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            enrichedLogger.Error(ex, "Request processing failed for {Method} {Path} after {ElapsedMs}ms",
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

        if (string.IsNullOrEmpty(queryString))
        {
            enrichedLogger.Information("HTTP {Method} request to {Path}",
                method, path);
        }
        else
        {
            enrichedLogger.Information("HTTP {Method} request to {Path} with query {QueryString}",
                method, path, queryString);
        }

        if (BodyCaptureMethods.Contains(method.ToUpperInvariant()) && 
            request.ContentLength > 0 && 
            request.ContentType?.Contains("application/json") == true)
        {
            var requestBody = await ReadRequestBodyAsync(request);
            if (!string.IsNullOrEmpty(requestBody))
            {
                enrichedLogger.Information("Request body: {RequestBody}", requestBody);
            }
        }
    }

    private static void LogResponse(HttpContext context, Serilog.ILogger enrichedLogger, long elapsedMilliseconds)
    {
        var response = context.Response;
        var statusCode = response.StatusCode;
        var method = context.Request.Method;
        var path = context.Request.Path;

        var statusCategory = GetStatusCategory(statusCode);

        if (statusCode >= 400)
        {
            enrichedLogger.Warning("HTTP {Method} {Path} completed with {StatusCode} {StatusCategory} in {ElapsedMs}ms",
                method, path, statusCode, statusCategory, elapsedMilliseconds);
        }
        else
        {
            enrichedLogger.Information("HTTP {Method} {Path} completed with {StatusCode} {StatusCategory} in {ElapsedMs}ms",
                method, path, statusCode, statusCategory, elapsedMilliseconds);
        }

        if (statusCode >= 400 || elapsedMilliseconds > 1000)
        {
            enrichedLogger.Debug("Response details: ContentType={ContentType}, ContentLength={ContentLength}",
                response.ContentType ?? "N/A",
                response.ContentLength ?? 0);
        }
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
}