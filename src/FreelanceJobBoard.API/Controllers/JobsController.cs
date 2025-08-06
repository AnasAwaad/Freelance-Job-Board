using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.DeleteJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetAllJobs;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobsByCurrentClient;
using FreelanceJobBoard.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class JobsController(IMediator mediator) : ControllerBase
{
	[HttpPost]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> Create([FromBody] CreateJobCommand command)
	{
		var id = await mediator.Send(command);

		return CreatedAtAction(nameof(GetById), new { id }, null);
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

	[HttpGet("{id}")]
	//[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> GetById([FromRoute] int id)
	{
		return Ok(await mediator.Send(new GetJobByIdQuery(id)));
	}

}
