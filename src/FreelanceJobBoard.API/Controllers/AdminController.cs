using FreelanceJobBoard.Application.Features.Admin.Commands.UpdateJobStatus;
using FreelanceJobBoard.Application.Features.Admin.Queries.GetAllJobs;
using FreelanceJobBoard.Application.Features.Admin.Queries.GetJobWithDetails;
using FreelanceJobBoard.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = AppRoles.Admin)]
public class AdminController(IMediator mediator) : ControllerBase
{

	[HttpPost("jobs/{jobId}/approve")]
	public async Task<IActionResult> ApproveJob([FromRoute] int jobId, [FromBody] ApprovalRequest request)
	{
		//string adminId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
		string adminId = Convert.ToString(1);
		await mediator.Send(new UpdateJobStatusCommand(jobId, JobStatus.Open, adminId, request?.Message ?? string.Empty));

		return Ok("Job approved successfully.");
	}


	[HttpPost("jobs/{jobId}/reject")]
	public async Task<IActionResult> RejectJob([FromRoute] int jobId, [FromBody] ApprovalRequest request)
	{
		//string adminId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
		string adminId = Convert.ToString(1);

		await mediator.Send(new UpdateJobStatusCommand(jobId, JobStatus.Cancelled, adminId, request?.Message ?? string.Empty));

		return Ok("Job rejected successfully.");
	}

	[HttpGet("jobs/{jobId}/details")]
	public async Task<IActionResult> GetJobDetails(int jobId)
	{
		var jobDetails = await mediator.Send(new GetJobDetailsWithHistoryQuery(jobId));
		return Ok(jobDetails);
	}

	[HttpGet("jobs")]
	public async Task<IActionResult> GetAllJobs([FromQuery] string? status)
	{
		return Ok(await mediator.Send(new GetAllJobsWithStatusQuery(status)));
	}
}

public class ApprovalRequest
{
	public string? Message { get; set; }
}
