using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.DeleteJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetAllJobs;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobsByCurrentClient;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetPublicJobDeatils;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetRecentJobs;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetRelatedJobs;
using FreelanceJobBoard.Application.Features.Jobs.Queries.SearchForJobs;
using FreelanceJobBoard.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class JobsController(IMediator mediator) : ControllerBase
{

	[HttpGet("recent-jobs/{numOfJobs}")]
	public async Task<IActionResult> GetRecentJobs(int numOfJobs)
	{
		var jobs = await mediator.Send(new GetRecentJobsQuery(numOfJobs));
		return Ok(jobs);
	}

	[HttpPost]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> Create([FromBody] CreateJobCommand command)
	{
		var id = await mediator.Send(command);

		return CreatedAtAction(nameof(GetById), new { id }, id);
	}

	[HttpPut("{id}")]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> Update([FromRoute] int id, UpdateJobCommand command)
	{
		command.Id = id;
		await mediator.Send(command);
		return NoContent();
	}

	[HttpDelete("{id}")]
	[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> Delete([FromRoute] int id)
	{
		await mediator.Send(new DeleteJobCommand(id));
		return NoContent();
	}

	[HttpGet]
	//[Authorize(Roles = AppRoles.Admin)]
	public async Task<IActionResult> GetAll([FromQuery] GetAllJobsQuery query)
	{
		var jobs = await mediator.Send(query);
		return Ok(jobs);
	}

	[HttpGet("my-jobs")]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> GetMyJobs([FromQuery] GetJobsByCurrentClientQuery query)
	{
		var jobs = await mediator.Send(query);
		return Ok(jobs);
	}

	[HttpGet("search")]

	public async Task<IActionResult> SearchJobsAsync([FromQuery] string query, [FromQuery] int limit = 20)
	{
		var result = await mediator.Send(new SearchJobsQuery(query, limit));
		return Ok(result);

	}

	[HttpGet("{id}")]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> GetById([FromRoute] int id)
	{
		return Ok(await mediator.Send(new GetJobByIdQuery(id)));
	}

	[HttpGet("details/{jobId}")]
	[AllowAnonymous]
	public async Task<IActionResult> GetPublicJobDetails(int jobId)
	{
		var job = await mediator.Send(new GetPublicJobDetailsByIdQuery(jobId));
		return Ok(job);
	}


	[HttpGet("related-jobs/{jobId}")]
	public async Task<IActionResult> GetRelatedJobsForJob(int jobId)
	{
		var job = await mediator.Send(new GetRetatedJobsForJobQuery(jobId));
		return Ok(job);
	}

}
