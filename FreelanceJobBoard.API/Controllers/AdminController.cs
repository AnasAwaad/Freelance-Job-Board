using FreelanceJobBoard.Application.Features.Admin.Commands.UpdateJobStatus;
using FreelanceJobBoard.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AdminController(IMediator mediator) : ControllerBase
{

	[HttpPost("jobs/{jobId}/approve")]
	public async Task<IActionResult> ApproveJob([FromRoute] int jobId)
	{
		//TODO :  Replace this with the actual authenticated Admin ID when auth is added
		int adminId = 1;
		await mediator.Send(new UpdateJobStatusCommand(jobId, JobStatus.Approved, adminId));

		return Ok("Job approved successfully.");
	}


	[HttpPost("jobs/{jobId}/reject")]
	public async Task<IActionResult> RejectJob(int jobId)
	{
		//TODO :  Replace this with the actual authenticated Admin ID when auth is added

		int adminId = 1;
		await mediator.Send(new UpdateJobStatusCommand(jobId, JobStatus.Rejected, adminId));


		return Ok("Job rejected successfully.");
	}
}
