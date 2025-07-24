using Serilog;
using System.Diagnostics;
using System.Text;

namespace FreelanceJobBoard.API.Middlewares;

public class RequestResponseLoggingMiddleware : IMiddleware
{
    private static readonly string[] BodyCaptureMethods = ["POST", "PUT", "PATCH"];
    private const int MaxBodyLength = 4096; 

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        var enrichedLogger = Log.ForContext("RequestId", requestId)
                               .ForContext("RemoteIpAddress", context.Connection.RemoteIpAddress?.ToString())
                               .ForContext("UserAgent", context.Request.Headers.UserAgent.ToString())
                               .ForContext("UserId", context.User?.Identity?.Name ?? "Anonymous");

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

            enrichedLogger.Error(ex, "Request processing failed for {Method} {Path} after {ElapsedMilliseconds}ms",
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
        var queryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;

        string? requestBody = null;

        if (BodyCaptureMethods.Contains(method.ToUpperInvariant()) && 
            request.ContentLength > 0 && 
            request.ContentType?.Contains("application/json") == true)
        {
            requestBody = await ReadRequestBodyAsync(request);
        }

        enrichedLogger.Information("Incoming {Method} request to {Path}{QueryString} with Content-Type: {ContentType} and Content-Length: {ContentLength}",
            method,
            path,
            queryString,
            request.ContentType ?? "N/A",
            request.ContentLength ?? 0);

        if (!string.IsNullOrEmpty(requestBody))
        {
            enrichedLogger.Information("Request body: {RequestBody}", requestBody);
        }
    }

    private static void LogResponse(HttpContext context, Serilog.ILogger enrichedLogger, long elapsedMilliseconds)
    {
        var response = context.Response;
        var statusCode = response.StatusCode;
        var method = context.Request.Method;
        var path = context.Request.Path;

        if (statusCode >= 400)
        {
            enrichedLogger.Warning("Outgoing {Method} response for {Path} returned {StatusCode} in {ElapsedMilliseconds}ms",
                method,
                path,
                statusCode,
                elapsedMilliseconds);
        }
        else
        {
            enrichedLogger.Information("Outgoing {Method} response for {Path} returned {StatusCode} in {ElapsedMilliseconds}ms",
                method,
                path,
                statusCode,
                elapsedMilliseconds);
        }

        enrichedLogger.ForContext("ResponseHeaders", response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))
                     .ForContext("ResponseContentType", response.ContentType ?? "N/A")
                     .ForContext("ResponseContentLength", response.ContentLength ?? 0)
                     .Debug("Response metadata logged");
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
                return body[..MaxBodyLength] + "... [TRUNCATED]";
            }

            return body;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to read request body");
            return "[FAILED TO READ BODY]";
        }
    }
}