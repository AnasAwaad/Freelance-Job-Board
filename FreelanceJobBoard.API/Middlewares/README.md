# Request/Response Logging Middleware

## Overview

The `RequestResponseLoggingMiddleware` is a comprehensive ASP.NET Core middleware that provides structured logging for all incoming HTTP requests and outgoing HTTP responses using Serilog. It captures essential request/response information and provides enriched logging context for better observability.

## Features

- **Structured Logging**: Uses Serilog.ForContext(...) for enriched structured logging
- **Request Body Capture**: Logs request body for POST, PUT, and PATCH requests (JSON content only)
- **Performance Monitoring**: Tracks total request processing time in milliseconds
- **Context Enrichment**: Adds RequestId, RemoteIpAddress, UserAgent, and UserId to log context
- **Exception Handling**: Gracefully handles and logs exceptions during request processing
- **Configurable Body Logging**: Limits request body size to prevent excessive logging (4KB limit)
- **Response Status Awareness**: Different log levels based on HTTP status codes (4xx/5xx = Warning, others = Information)

## Installation & Setup

### 1. Required Packages

The following packages are required and should already be installed:
<PackageReference Include="Serilog" Version="4.3.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
### 2. Middleware Registration

The middleware is already registered in your `Program.cs`:
// Register the middleware in the DI container
builder.Services.AddScoped<RequestResponseLoggingMiddleware>();

// Add to the middleware pipeline (should be early in the pipeline)
app.UseMiddleware<RequestResponseLoggingMiddleware>();
### 3. Serilog Configuration

Your application is configured to use Serilog with the following setup:

**appsettings.json:**{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext"],
    "Properties": {
      "ApplicationName": "FreelanceJobBoard.API"
    }
  }
}
## What Gets Logged

### Request Information (Information Level)

- HTTP Method (GET, POST, PUT, DELETE, etc.)
- Request Path and Query String
- Content-Type and Content-Length headers
- Request Body (for POST, PUT, PATCH with `application/json` content type only)
- Unique Request ID for correlation
- Remote IP Address
- User Agent
- User Identity (if authenticated, otherwise "Anonymous")

### Response Information (Information/Warning Level)

- HTTP Status Code
- Processing Time in milliseconds
- Response Headers (debug level only)
- Response Content-Type and Content-Length
- Request/Response correlation via Request ID

### Exception Information (Error Level)

- Full exception details with stack trace
- Request context when exception occurred
- Processing time until exception
- All enriched context properties

## Sample Log Output

### Successful POST Request2024-12-19 10:15:23.456 +00:00 [INF] Incoming POST request to /api/categories with Content-Type: application/json and Content-Length: 45 {"RequestId": "a1b2c3d4", "RemoteIpAddress": "127.0.0.1", "UserAgent": "Mozilla/5.0", "UserId": "Anonymous", "ApplicationName": "FreelanceJobBoard.API"}

2024-12-19 10:15:23.457 +00:00 [INF] Request body: {"name":"Web Development","description":"Web development services"} {"RequestId": "a1b2c3d4", "RemoteIpAddress": "127.0.0.1", "UserAgent": "Mozilla/5.0", "UserId": "Anonymous", "ApplicationName": "FreelanceJobBoard.API"}

2024-12-19 10:15:23.523 +00:00 [INF] Outgoing POST response for /api/categories returned 201 in 67ms {"RequestId": "a1b2c3d4", "RemoteIpAddress": "127.0.0.1", "UserAgent": "Mozilla/5.0", "UserId": "Anonymous", "ApplicationName": "FreelanceJobBoard.API"}
### GET Request (No Body)2024-12-19 10:16:12.123 +00:00 [INF] Incoming GET request to /api/categories?page=1 with Content-Type: N/A and Content-Length: 0 {"RequestId": "b5c6d7e8", "RemoteIpAddress": "127.0.0.1", "UserAgent": "PostmanRuntime/7.32.3", "UserId": "john.doe", "ApplicationName": "FreelanceJobBoard.API"}

2024-12-19 10:16:12.145 +00:00 [INF] Outgoing GET response for /api/categories returned 200 in 22ms {"RequestId": "b5c6d7e8", "RemoteIpAddress": "127.0.0.1", "UserAgent": "PostmanRuntime/7.32.3", "UserId": "john.doe", "ApplicationName": "FreelanceJobBoard.API"}
### Error Response (404)2024-12-19 10:17:24.123 +00:00 [INF] Incoming GET request to /api/categories/9999 with Content-Type: N/A and Content-Length: 0 {"RequestId": "e5f6g7h8", "RemoteIpAddress": "127.0.0.1", "UserAgent": "PostmanRuntime/7.32.3", "UserId": "Anonymous", "ApplicationName": "FreelanceJobBoard.API"}

2024-12-19 10:17:24.135 +00:00 [WAR] Outgoing GET response for /api/categories/9999 returned 404 in 12ms {"RequestId": "e5f6g7h8", "RemoteIpAddress": "127.0.0.1", "UserAgent": "PostmanRuntime/7.32.3", "UserId": "Anonymous", "ApplicationName": "FreelanceJobBoard.API"}
### Exception Handling2024-12-19 10:18:25.789 +00:00 [INF] Incoming POST request to /api/categories with Content-Type: application/json and Content-Length: 45 {"RequestId": "i9j0k1l2", "RemoteIpAddress": "127.0.0.1", "UserAgent": "PostmanRuntime/7.32.3", "UserId": "Anonymous", "ApplicationName": "FreelanceJobBoard.API"}

2024-12-19 10:18:25.834 +00:00 [ERR] Request processing failed for POST /api/categories after 45ms {"RequestId": "i9j0k1l2", "RemoteIpAddress": "127.0.0.1", "UserAgent": "PostmanRuntime/7.32.3", "UserId": "Anonymous", "ApplicationName": "FreelanceJobBoard.API"}
System.InvalidOperationException: Database connection failed
   at FreelanceJobBoard.Infrastructure.Repositories.CategoryRepository.CreateAsync...
## Configuration Options

### Body Capture Settings

The middleware includes configurable constants in the source code:
private static readonly string[] BodyCaptureMethods = ["POST", "PUT", "PATCH"];
private const int MaxBodyLength = 4096; // 4KB limit for request body logging
### Log Level Configuration

You can adjust log levels in `appsettings.json`:
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "FreelanceJobBoard.API.Middlewares.RequestResponseLoggingMiddleware": "Debug"
      }
    }
  }
}
### Development vs Production

- **Development**: Uses more detailed logging with shorter timestamps for console output
- **Production**: Uses full timestamps and retains log files for 7 days

## Performance Considerations

- **Selective Body Capture**: Only captures request bodies for POST/PUT/PATCH with JSON content
- **Size Limits**: Request body logging is limited to 4KB to prevent memory issues
- **Buffering**: Uses efficient stream buffering to read request bodies
- **Async Processing**: All I/O operations are properly awaited
- **Exception Safety**: Middleware handles all errors gracefully and continues processing

## Security Considerations

?? **Important Security Notes:**

1. **Sensitive Data**: Request bodies are logged in plain text. Avoid logging sensitive data like passwords, tokens, or PII
2. **Log Access**: Ensure log files have appropriate access controls
3. **Log Retention**: Configure appropriate retention policies for compliance
4. **Data Masking**: Consider implementing custom logic to mask sensitive fields

## Integration with Error Handling

The middleware works seamlessly with your existing `ErrorHandlingMiddleware`:

1. **Request/Response Logging** ? Logs all requests and responses
2. **Exception Occurs** ? Logged by RequestResponseLoggingMiddleware with context
3. **Error Handling** ? Your ErrorHandlingMiddleware processes the exception
4. **Final Response** ? Already logged with appropriate status code

## Middleware Pipeline Order
app.UseMiddleware<RequestResponseLoggingMiddleware>(); // First - logs everything
app.UseMiddleware<ErrorHandlingMiddleware>();          // Second - handles errors
// ... other middleware
## Troubleshooting

### Common Issues

1. **No Logs Appearing**
   - Verify Serilog configuration in appsettings.json
   - Check minimum log level allows Information level
   - Ensure logs directory exists and is writable

2. **Request Body Not Logged**
   - Verify request method is POST, PUT, or PATCH
   - Check Content-Type is `application/json`
   - Ensure request has content (Content-Length > 0)

3. **"Synchronous operations are disallowed" Error**
   - ? **FIXED**: This error occurred in previous versions where request body was read synchronously
   - The middleware now uses `ReadToEndAsync()` for proper async operations
   - If you see this error, ensure you're using the latest version of the middleware

4. **Performance Issues**
   - Monitor log file sizes
   - Adjust `MaxBodyLength` if needed
   - Consider filtering out health check endpoints

5. **Missing Context Properties**
   - Verify `Enrich.FromLogContext()` is configured
   - Check middleware registration order

6. **Request Body Shows "[FAILED TO READ BODY]"**
   - Check the logs for the specific exception details
   - Verify the request body is valid JSON
   - Ensure the Content-Type header is set correctly

### Log File Location

Log files are created in: `logs/app-{date}.log`

Example: `logs/app-20241219.log`

### Expected Behavior After Fix

After the async fix, you should see proper request body logging:2024-12-19 15:49:02.970 [INF] Incoming PUT request to /api/Categories/1 with Content-Type: application/json and Content-Length: 45 {"RequestId": "7d80a758", ...}
2024-12-19 15:49:02.977 [INF] Request body: {"name":"Updated Category","description":"Updated description"} {"RequestId": "7d80a758", ...}
Instead of the previous error:2024-12-19 15:49:02.945 [WRN] Failed to read request body
System.InvalidOperationException: Synchronous operations are disallowed...
2024-12-19 15:49:02.977 [INF] Request body: [FAILED TO READ BODY]## Testing the Middleware

You can test the middleware by making requests to your API endpoints:
# GET request (no body logged)
curl -X GET "https://localhost:7000/api/categories"

# POST request (body will be logged)
curl -X POST "https://localhost:7000/api/categories" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Category","description":"Test Description"}'
Check the console output or log files to see the structured logging in action!