# Request/Response Logging Middleware

## Overview

The `RequestResponseLoggingMiddleware` provides comprehensive structured logging for HTTP requests and responses in ASP.NET Core applications using Serilog. This middleware is designed for production environments with professional logging standards and minimal performance overhead.

## Features

- **Structured Logging**: Utilizes Serilog with contextual enrichment for detailed request tracking
- **Request Body Capture**: Logs request payloads for POST, PUT, and PATCH operations with JSON content
- **Performance Monitoring**: Tracks request processing time with millisecond precision
- **Smart Filtering**: Automatically excludes static resources and infrastructure endpoints
- **Error Context**: Provides detailed context for failed requests and exceptions
- **Configurable Output**: Separate formatting for console and file outputs

## Architecture

### Middleware Pipeline Position
The middleware should be positioned early in the request pipeline to capture all requests before they reach other middleware components.
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
### Configuration

#### Serilog Configuration
The middleware leverages Serilog's structured logging capabilities with the following configuration:

**Production Environment:**{
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
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/application-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj} | RequestId: {RequestId} | UserId: {UserId} | RemoteIP: {RemoteIP}{NewLine}{Exception}"
        }
      }
    ]
  }
}
## Logging Output

### Request Logging
All incoming HTTP requests are logged with the following information:
- HTTP method and path
- Query string parameters (if present)
- Request body (for POST/PUT/PATCH with JSON content)
- Request correlation ID
- User identity
- Remote IP address

### Response Logging
All HTTP responses are logged with:
- HTTP status code and category
- Request processing time
- Response metadata for errors or slow requests

### Sample Output

**Successful Request:**[15:30:45 INF] HTTP GET request to /api/categories
[15:30:45 INF] HTTP GET /api/categories completed with 200 SUCCESS in 23ms
**Request with Body:**[15:31:12 INF] HTTP POST request to /api/categories
[15:31:12 INF] Request body: {"name":"Web Development","description":"Professional web development services"}
[15:31:12 INF] HTTP POST /api/categories completed with 201 SUCCESS in 67ms
**Error Response:**[15:32:05 INF] HTTP GET request to /api/categories/999
[15:32:05 WRN] HTTP GET /api/categories/999 completed with 404 CLIENT_ERROR in 12ms
**Exception Handling:**[15:33:01 INF] HTTP POST request to /api/categories
[15:33:01 INF] Request body: {"name":"Test Category"}
[15:33:01 ERR] Request processing failed for POST /api/categories after 145ms
System.InvalidOperationException: Database connection failed
   at FreelanceJobBoard.Infrastructure.Repositories.CategoryRepository.CreateAsync...
## Configuration Options

### Request Body Logging
- **Methods**: POST, PUT, PATCH
- **Content Type**: application/json
- **Size Limit**: 2048 characters (configurable via `MaxBodyLength`)
- **Truncation**: Bodies exceeding the limit are truncated with "[TRUNCATED]" indicator

### Filtered Endpoints
The middleware automatically excludes the following from logging:
- Static files (.css, .js, .ico, .png, .jpg, .jpeg, .gif, .svg, .woff, .woff2, .ttf)
- Swagger UI endpoints (/swagger/)
- Framework files (/_framework/)
- Health check endpoints (/health)
- Favicon requests (/favicon)

### Status Categories
HTTP responses are categorized as follows:
- **SUCCESS**: 200-299
- **REDIRECT**: 300-399
- **CLIENT_ERROR**: 400-499
- **SERVER_ERROR**: 500-599
- **INFORMATIONAL**: 100-199

## Performance Considerations

### Optimizations
- Asynchronous request body reading
- Smart filtering to reduce log volume
- Configurable body size limits
- Efficient stream buffering
- Minimal memory allocation

### Monitoring
- Response times over 1000ms trigger additional debug logging
- Error responses include extended context information
- Request correlation tracking for distributed systems

## Security Considerations

### Data Protection
- Request bodies are logged in plain text
- Implement additional filtering for sensitive endpoints
- Consider data masking for personally identifiable information
- Ensure appropriate log file access controls

### Compliance
- Configure log retention policies according to organizational requirements
- Implement log rotation to manage storage usage
- Consider encryption for log files in sensitive environments

## Integration

The middleware integrates seamlessly with existing ASP.NET Core applications and works alongside other middleware components. It provides structured logging data that can be consumed by log aggregation systems like ELK Stack, Splunk, or Azure Application Insights.

### Dependencies
- Serilog.AspNetCore
- Microsoft.AspNetCore.Http
- System.Text.Json (for JSON content type detection)

## Troubleshooting

### Common Issues

**No Request Bodies Logged**
- Verify Content-Type is application/json
- Check that Content-Length is greater than 0
- Confirm request method is POST, PUT, or PATCH

**High Log Volume**
- Review filtered endpoints configuration
- Adjust minimum log levels in appsettings.json
- Consider implementing custom filtering logic

**Performance Impact**
- Monitor application performance metrics
- Adjust MaxBodyLength if processing large request bodies
- Consider disabling body logging for high-throughput endpoints

### Log File Management
- Files are created in the logs/ directory
- Daily rotation prevents excessive file sizes
- Configurable retention policies manage storage usage

This middleware provides enterprise-grade logging capabilities while maintaining optimal performance characteristics for production environments.