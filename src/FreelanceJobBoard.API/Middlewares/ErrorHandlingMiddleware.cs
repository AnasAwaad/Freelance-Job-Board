using FreelanceJobBoard.Domain.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace FreelanceJobBoard.API.Middlewares;

public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next.Invoke(context);
		}
		catch (ValidationException validationEx)
		{
			logger.LogWarning("Validation failed: {Errors}", string.Join(", ", validationEx.Errors.Select(e => e.ErrorMessage)));
			
			context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			context.Response.ContentType = "application/json";
			
			var response = new
			{
				type = "Validation Error",
				title = "One or more validation errors occurred.",
				status = 400,
				errors = validationEx.Errors.GroupBy(e => e.PropertyName)
					.ToDictionary(
						g => g.Key,
						g => g.Select(e => e.ErrorMessage).ToArray()
					)
			};
			
			var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
			
			await context.Response.WriteAsync(jsonResponse);
		}
		catch (InvalidOperationException invalidOpEx)
		{
			logger.LogWarning("Invalid operation: {Message}", invalidOpEx.Message);
			
			context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			context.Response.ContentType = "application/json";
			
			var response = new
			{
				type = "Invalid Operation",
				title = invalidOpEx.Message,
				status = 400
			};
			
			var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
			
			await context.Response.WriteAsync(jsonResponse);
		}
		catch (ArgumentException argumentEx)
		{
			logger.LogWarning("Invalid argument: {Message}", argumentEx.Message);
			
			context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
			context.Response.ContentType = "application/json";
			
			var response = new
			{
				type = "Bad Request",
				title = argumentEx.Message,
				status = 400
			};
			
			var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
			
			await context.Response.WriteAsync(jsonResponse);
		}
		catch (UnauthorizedAccessException unauthorizedEx)
		{
			logger.LogWarning("Unauthorized access: {Message}", unauthorizedEx.Message);
			
			context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			context.Response.ContentType = "application/json";
			
			var response = new
			{
				type = "Unauthorized",
				title = unauthorizedEx.Message,
				status = 401
			};
			
			var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
			
			await context.Response.WriteAsync(jsonResponse);
		}
		catch (NotFoundException notFound)
		{
			logger.LogWarning("Resource not found: {Message}", notFound.Message);
			
			context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			context.Response.ContentType = "application/json";
			
			var response = new
			{
				type = "Not Found",
				title = notFound.Message,
				status = 404
			};
			
			var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
			
			await context.Response.WriteAsync(jsonResponse);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An unexpected error occurred: {Message}", ex.Message);

			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			context.Response.ContentType = "application/json";
			
			var response = new
			{
				type = "Internal Server Error",
				title = "An unexpected error occurred.",
				status = 500
			};
			
			var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
			
			await context.Response.WriteAsync(jsonResponse);
		}
	}
}