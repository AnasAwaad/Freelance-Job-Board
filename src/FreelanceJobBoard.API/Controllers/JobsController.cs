using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.DeleteJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetAllJobs;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobsByCurrentClient;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobsByCurrentFreelancer;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetPublicJobDeatils;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetRecentJobs;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetRelatedJobs;
using FreelanceJobBoard.Application.Features.Jobs.Queries.SearchForJobs;
using FreelanceJobBoard.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace FreelanceJobBoard.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<JobsController> _logger;

	public JobsController(IMediator mediator, ILogger<JobsController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}


	[HttpGet("recent-jobs/{numOfJobs}")]
	public async Task<IActionResult> GetRecentJobs(int numOfJobs)
	{
		var jobs = await _mediator.Send(new GetRecentJobsQuery(numOfJobs));
		return Ok(jobs);
	}

	[HttpPost]
	[Authorize(Roles = AppRoles.Client)]
	//[Authorize]
	public async Task<IActionResult> Create([FromBody] CreateJobCommand command)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🚀 Starting job creation for user {UserId} | RequestId: {RequestId}", userId, requestId);
			_logger.LogInformation("📝 Job Details: Title='{JobTitle}', Budget=${BudgetMin}-${BudgetMax}, Skills={SkillCount}, Categories={CategoryCount}",
				command.Title, command.BudgetMin, command.BudgetMax,
				command.SkillIds?.Count() ?? 0, command.CategoryIds?.Count() ?? 0);

			// Log request headers for debugging
			LogRequestHeaders("CREATE_JOB");

			_logger.LogDebug("📋 Full job creation command: {@Command}", command);

			command.UserId = userId;
			var id = await _mediator.Send(command);

			stopwatch.Stop();
			_logger.LogInformation("✅ Job created successfully! JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);

			// Log successful response details
			_logger.LogDebug("📤 Response: CreatedAtAction with JobId={JobId}", id);

			return CreatedAtAction(nameof(GetById), new { id }, id);
		}
		catch (UnauthorizedAccessException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized job creation attempt | User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage} | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return Unauthorized();
		}
		catch (ArgumentException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("⚠️ Invalid job creation data | User={UserId}, Duration={ElapsedMs}ms, ValidationError='{ErrorMessage}' | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Job creation failed! User={UserId}, Title='{JobTitle}', Duration={ElapsedMs}ms | RequestId: {RequestId}",
				userId, command.Title, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while creating the job");
		}
	}

	[HttpPut("{id}")]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> Update([FromRoute] int id, UpdateJobCommand command)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🔄 Starting job update | JobId={JobId}, User={UserId} | RequestId: {RequestId}", id, userId, requestId);

			command.Id = id;
			_logger.LogInformation("📝 Update Details: Title='{JobTitle}', Budget=${BudgetMin}-${BudgetMax}",
				command.Title, command.BudgetMin, command.BudgetMax);

			LogRequestHeaders("UPDATE_JOB");
			_logger.LogDebug("📋 Full job update command: {@Command}", command);

			await _mediator.Send(command);

			stopwatch.Stop();
			_logger.LogInformation("✅ Job updated successfully! JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);

			return NoContent();
		}
		catch (UnauthorizedAccessException)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized job update attempt | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return Unauthorized();
		}
		catch (Domain.Exceptions.NotFoundException)
		{
			stopwatch.Stop();
			_logger.LogWarning("❌ Job not found for update | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return NotFound();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Job update failed! JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while updating the job");
		}
	}

	[HttpDelete("{id}")]
	[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> Delete([FromRoute] int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🗑️ Starting job deletion | JobId={JobId}, User={UserId} | RequestId: {RequestId}", id, userId, requestId);

			LogRequestHeaders("DELETE_JOB");

			await _mediator.Send(new DeleteJobCommand(id));

			stopwatch.Stop();
			_logger.LogInformation("✅ Job deleted successfully! JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);

			return NoContent();
		}
		catch (UnauthorizedAccessException)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized job deletion attempt | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return Unauthorized();
		}
		catch (Domain.Exceptions.NotFoundException)
		{
			stopwatch.Stop();
			_logger.LogWarning("❌ Job not found for deletion | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return NotFound();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Job deletion failed! JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while deleting the job");
		}
	}

	[HttpGet]
	//[Authorize(Roles = AppRoles.Admin)]
	public async Task<IActionResult> GetAll([FromQuery] GetAllJobsQuery query)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("📥 Retrieving all jobs | User={UserId}, Page={PageNumber}, Size={PageSize}, Search='{Search}' | RequestId: {RequestId}",
				userId, query.PageNumber, query.PageSize, query.Search ?? "none", requestId);

			LogRequestHeaders("GET_ALL_JOBS");

			var jobs = await _mediator.Send(query);

			stopwatch.Stop();
			var jobCount = jobs.Items?.Count() ?? 0;
			_logger.LogInformation("✅ Retrieved jobs successfully! Count={JobCount}/{TotalCount}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				jobCount, jobs.TotalItemsCount, userId, stopwatch.ElapsedMilliseconds, requestId);

			// Log pagination info
			_logger.LogDebug("📊 Pagination: TotalPages={TotalPages}, ItemsFrom={ItemsFrom}, ItemsTo={ItemsTo}",
				jobs.TotalPages, jobs.ItemsFrom, jobs.ItemsTo);

			return Ok(jobs);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Failed to retrieve jobs! User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while retrieving jobs");
		}
	}

	[HttpGet("my-jobs")]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> GetMyJobs([FromQuery] GetJobsByCurrentClientQuery query)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("👤 Retrieving client jobs | User={UserId} | RequestId: {RequestId}", userId, requestId);

			LogRequestHeaders("GET_MY_JOBS");

			var jobs = await _mediator.Send(query);

			stopwatch.Stop();
			var jobCount = jobs?.Count() ?? 0;
			_logger.LogInformation("✅ Retrieved client jobs successfully! Count={JobCount}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				jobCount, userId, stopwatch.ElapsedMilliseconds, requestId);

			return Ok(jobs);
		}
		catch (UnauthorizedAccessException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized access to client jobs | User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage} | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return Unauthorized();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Failed to retrieve client jobs! User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while retrieving your jobs");
		}
	}

	[HttpGet("my-freelancer-jobs")]
	//[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> GetMyFreelancerJobs([FromQuery] GetJobsByCurrentFreelancerQuery query)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("💼 Retrieving freelancer jobs | User={UserId} | RequestId: {RequestId}", userId, requestId);

			LogRequestHeaders("GET_FREELANCER_JOBS");

			var jobs = await _mediator.Send(query);

			stopwatch.Stop();
			var jobCount = jobs?.Count() ?? 0;
			_logger.LogInformation("✅ Retrieved freelancer jobs successfully! Count={JobCount}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				jobCount, userId, stopwatch.ElapsedMilliseconds, requestId);

			return Ok(jobs);
		}
		catch (UnauthorizedAccessException ex)
		{
			stopwatch.Stop();
			_logger.LogWarning("🚫 Unauthorized access to freelancer jobs | User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage} | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, ex.Message, requestId);
			return Unauthorized();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Failed to retrieve freelancer jobs! User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while retrieving available jobs");
		}
	}

	[HttpGet("search")]

	public async Task<IActionResult> SearchJobsAsync([FromQuery] string query, [FromQuery] int limit = 20)
	{
		var result = await _mediator.Send(new SearchJobsQuery(query, limit));
		return Ok(result);

	}

	[HttpGet("{id}")]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> GetById([FromRoute] int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = User?.Identity?.Name ?? "Anonymous";
		var requestId = HttpContext.TraceIdentifier;

		try
		{
			_logger.LogInformation("🔍 Retrieving job details | JobId={JobId}, User={UserId} | RequestId: {RequestId}", id, userId, requestId);

			LogRequestHeaders("GET_JOB_BY_ID");

			var job = await _mediator.Send(new GetJobByIdQuery(id));

			stopwatch.Stop();
			_logger.LogInformation("✅ Job retrieved successfully! JobId={JobId}, Title='{JobTitle}', User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, job.Title, userId, stopwatch.ElapsedMilliseconds, requestId);

			// Log job details for debugging
			_logger.LogDebug("📋 Job Details: Status='{Status}', Budget=${BudgetMin}-${BudgetMax}, Deadline={Deadline}",
				job.Status, job.BudgetMin, job.BudgetMax, job.Deadline);

			return Ok(job);
		}
		catch (Domain.Exceptions.NotFoundException)
		{
			stopwatch.Stop();
			_logger.LogWarning("❌ Job not found | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return NotFound();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Failed to retrieve job! JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms | RequestId: {RequestId}",
				id, userId, stopwatch.ElapsedMilliseconds, requestId);
			return StatusCode(500, "An error occurred while retrieving the job");
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

			// Log specific headers
			if (HttpContext.Request.Headers.ContainsKey("User-Agent"))
			{
				_logger.LogDebug("🌐 User-Agent: {UserAgent}", HttpContext.Request.Headers.UserAgent.ToString());
			}

			if (HttpContext.Request.Headers.ContainsKey("Content-Type"))
			{
				_logger.LogDebug("📄 Content-Type: {ContentType}", HttpContext.Request.Headers.ContentType.ToString());
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
			"Content-Length", "Accept-Encoding", "Accept-Language"
		};
		return important.Contains(headerName, StringComparer.OrdinalIgnoreCase);
	}

	[HttpGet("details/{jobId}")]
	[AllowAnonymous]
	public async Task<IActionResult> GetPublicJobDetails(int jobId)
	{
		var job = await _mediator.Send(new GetPublicJobDetailsByIdQuery(jobId));
		return Ok(job);
	}


	[HttpGet("related-jobs/{jobId}")]
	public async Task<IActionResult> GetRelatedJobsForJob(int jobId)
	{
		var job = await _mediator.Send(new GetRetatedJobsForJobQuery(jobId));
		return Ok(job);
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
