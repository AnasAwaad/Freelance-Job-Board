using FreelanceJobBoard.API.Attributes;
using FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
using FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
using FreelanceJobBoard.Application.Features.Categories.Commands.UpdateCategory;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetAllCategories;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetCategoryById;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetTopCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FreelanceJobBoard.API.Controllers;

//[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<CategoriesController> _logger;

	public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	[RateLimit(100, 60)] // Increased from 5 to 100 requests per minute - categories are read-only data that changes infrequently
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("📂 Retrieving all categories | User={UserId} | RequestId: {RequestId}", userId, requestId);

			LogRequestHeaders("GET_ALL_CATEGORIES");

			var categories = await _mediator.Send(new GetAllCategoriesQuery());

			stopwatch.Stop();
			var categoryCount = categories?.Count() ?? 0;
			_logger.LogInformation("✅ Categories retrieved successfully! Count={CategoryCount}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				categoryCount, userId, stopwatch.ElapsedMilliseconds, requestId);

			// Log category details for debugging
			if (categories?.Any() == true)
			{
				var activeCount = categories.Count(c => c.IsActive);
				_logger.LogDebug("📊 Category Stats: Total={Total}, Active={Active}, Inactive={Inactive}",
					categoryCount, activeCount, categoryCount - activeCount);
			}

			return Ok(categories);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Failed to retrieve categories! User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while retrieving categories");
		}
	}

	[RateLimit(50, 60)] // Also increase rate limit for top categories as it's frequently used
	[HttpGet("top/{numOfCategories}")]
	public async Task<IActionResult> GetTopCategories([FromRoute] int numOfCategories)
	{
		var result = await _mediator.Send(new GetTopCategoriesQuery(numOfCategories));
		return Ok(result);
	}
	
	[RateLimit(50, 60)] // Reasonable limit for individual category retrieval
	[HttpGet("{id}")]
	public async Task<IActionResult> GetById([FromRoute] int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🔍 Retrieving category details | CategoryId={CategoryId}, User={UserId} | RequestId: {RequestId}", id, userId, requestId);

			LogRequestHeaders("GET_CATEGORY_BY_ID");

			var category = await _mediator.Send(new GetCategoryByIdQuery(id));

			stopwatch.Stop();
			_logger.LogInformation("✅ Category retrieved successfully! CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, category.Name, userId, stopwatch.ElapsedMilliseconds, requestId);

			// Log category details
			_logger.LogDebug("📋 Category Details: Name='{Name}', Description='{Description}', Active={IsActive}",
				category.Name, category.Description, category.IsActive);

			return Ok(category);
		}
		catch (Domain.Exceptions.NotFoundException)
		{
			stopwatch.Stop();
			_logger.LogWarning("❌ Category not found | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return NotFound();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Failed to retrieve category! CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while retrieving the category");
		}
	}

	[RateLimit(10, 60)] // Keep stricter limits for write operations
	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🆕 Starting category creation | User={UserId} | RequestId: {RequestId}", userId, requestId);
			_logger.LogInformation("📝 Category Details: Name='{CategoryName}', Description='{Description}'",
				command.Name, command.Description);

			LogRequestHeaders("CREATE_CATEGORY");
			_logger.LogDebug("📋 Full category creation command: {@Command}", command);

			var category = await _mediator.Send(command);

			stopwatch.Stop();
			_logger.LogInformation("✅ Category created successfully! CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				category.Id, category.Name, userId, stopwatch.ElapsedMilliseconds, requestId);

			// Log response details
			_logger.LogDebug("📤 Response: CreatedAtAction with CategoryId={CategoryId}", category.Id);

			return CreatedAtAction(nameof(GetById), new { category.Id }, category);
		}
		catch (UnauthorizedAccessException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized category creation attempt | User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage} | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return Unauthorized();
		}
		catch (ArgumentException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("⚠️ Invalid category creation data | User={UserId}, Duration={ElapsedMs}ms, ValidationError='{ErrorMessage}' | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return BadRequest(new { message = ex.Message });
		}
		catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
		{
			stopwatch.Stop();
			_logger.LogWarning("🔄 Duplicate category creation attempt | Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				command.Name, userId, stopwatch.ElapsedMilliseconds, requestId);
			return Conflict(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Category creation failed! Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				command.Name, userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while creating the category");
		}
	}

	[RateLimit(10, 60)] // Keep stricter limits for write operations
	[HttpPut("{id}")]
	public async Task<IActionResult> Update([FromRoute] int id, UpdateCategoryCommand command)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🔄 Starting category update | CategoryId={CategoryId}, User={UserId} | RequestId: {RequestId}", id, userId, requestId);

			command.Id = id;
			_logger.LogInformation("📝 Update Details: Name='{CategoryName}', Description='{Description}'",
				command.Name, command.Description);

			LogRequestHeaders("UPDATE_CATEGORY");
			_logger.LogDebug("📋 Full category update command: {@Command}", command);

			var category = await _mediator.Send(command);

			stopwatch.Stop();
			_logger.LogInformation("✅ Category updated successfully! CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, category.Name, userId, stopwatch.ElapsedMilliseconds, requestId);

			return Ok(category);
		}
		catch (UnauthorizedAccessException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized category update attempt | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage} | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return Unauthorized();
		}
		catch (Domain.Exceptions.NotFoundException)
		{
			stopwatch.Stop();
			_logger.LogWarning("❌ Category not found for update | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return NotFound();
		}
		catch (ArgumentException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("⚠️ Invalid category update data | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms, ValidationError='{ErrorMessage}' | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Category update failed! CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while updating the category");
		}
	}

	[RateLimit(10, 60)] // Keep stricter limits for status changes
	[HttpPost("{id}/ChangeStatus")]
	public async Task<IActionResult> ChangeStatus([FromRoute] int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🔄 Starting category status change | CategoryId={CategoryId}, User={UserId} | RequestId: {RequestId}", id, userId, requestId);

			LogRequestHeaders("CHANGE_CATEGORY_STATUS");

			var result = await _mediator.Send(new ChangeCategoryStatusCommand(id));

			stopwatch.Stop();
			_logger.LogInformation("✅ Category status changed successfully! CategoryId={CategoryId}, NewStatus={IsActive}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, result.IsActive, userId, stopwatch.ElapsedMilliseconds, requestId);

			// Log status change details
			_logger.LogDebug("📊 Status Change: CategoryId={CategoryId}, IsActive={IsActive}, LastUpdated={LastUpdated}",
				id, result?.IsActive ?? false, result?.LastUpdatedOn.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown");

			return Ok(result);
		}
		catch (UnauthorizedAccessException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized category status change attempt | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage} | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return Unauthorized();
		}
		catch (Domain.Exceptions.NotFoundException)
		{
			stopwatch.Stop();
			_logger.LogWarning("❌ Category not found for status change | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return NotFound();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Category status change failed! CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while changing the category status");
		}
	}

	[RateLimit(5, 60)] // Keep strict limits for delete operations
	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete([FromRoute] int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🗑️ Starting category deletion | CategoryId={CategoryId}, User={UserId} | RequestId: {RequestId}", id, userId, requestId);

			LogRequestHeaders("DELETE_CATEGORY");

			var result = await _mediator.Send(new DeleteCategoryCommand(id));

			stopwatch.Stop();
			_logger.LogInformation("✅ Category deleted successfully! CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);

			return Ok(new { success = true, message = "Category deleted successfully" });
		}
		catch (UnauthorizedAccessException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized category deletion attempt | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage} | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return Unauthorized();
		}
		catch (Domain.Exceptions.NotFoundException)
		{
			stopwatch.Stop();
			_logger.LogWarning("❌ Category not found for deletion | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return NotFound(new { success = false, message = "Category not found" });
		}
		catch (InvalidOperationException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("⚠️ Cannot delete category with dependencies | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms, Error='{ErrorMessage}' | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return BadRequest(new { success = false, message = ex.Message });
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Category deletion failed! CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, new { success = false, message = "An error occurred while deleting the category" });
		}
	}

	private void LogRequestHeaders(string operation)
	{
		try
		{
			var headers = HttpContext.Request.Headers
				.Where(h => IsImportantHeader(h.Key))
				.ToDictionary(h => h.Key, h => GetSafeHeaderValue(h.Value.ToString()));

			if (headers.Any())
			{
				_logger.LogDebug("📋 {Operation} - Important Headers: {@Headers}", operation, headers);
			}

			// Log specific headers with emojis
			if (HttpContext.Request.Headers.ContainsKey("User-Agent"))
			{
				_logger.LogDebug("🌐 User-Agent: {UserAgent}", HttpContext.Request.Headers.UserAgent.ToString());
			}

			if (HttpContext.Request.Headers.ContainsKey("Content-Type"))
			{
				_logger.LogDebug("📄 Content-Type: {ContentType}", HttpContext.Request.Headers.ContentType.ToString());
			}

			if (HttpContext.Request.Headers.ContainsKey("Accept"))
			{
				_logger.LogDebug("🎯 Accept: {Accept}", HttpContext.Request.Headers.Accept.ToString());
			}

			// Log rate limiting headers if present
			if (HttpContext.Request.Headers.ContainsKey("X-RateLimit-Limit"))
			{
				_logger.LogDebug("⏱️ Rate Limit: Limit={Limit}, Remaining={Remaining}",
					HttpContext.Request.Headers["X-RateLimit-Limit"].ToString(),
					HttpContext.Request.Headers["X-RateLimit-Remaining"].ToString());
			}
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "Failed to log request headers for operation {Operation}", operation);
		}
	}

	private static bool IsImportantHeader(string headerName)
	{
		var important = new[]
		{
			"Content-Type", "Accept", "Authorization", "User-Agent",
			"Content-Length", "Accept-Encoding", "Accept-Language",
			"Cache-Control", "If-None-Match", "If-Modified-Since"
		};
		return important.Contains(headerName, StringComparer.OrdinalIgnoreCase);
	}

	private static string GetSafeHeaderValue(string headerValue)
	{
		if (string.IsNullOrEmpty(headerValue))
			return "";

		// Redact sensitive headers
		if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			return "Bearer [REDACTED]";

		return headerValue.Length > 100 ? headerValue[..100] + "[TRUNCATED]" : headerValue;
	}
}
