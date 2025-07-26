using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.DeleteJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetAllJobs;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class JobsController(IMediator mediator) : ControllerBase
{

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateJobCommand command)
	{
		await mediator.Send(command);
		//return CreatedAtAction(nameof(GetById), new { id }, null);
		return NoContent();
	}


	[HttpPut("{id}")]
	public async Task<IActionResult> Update([FromRoute] int id, UpdateJobCommand command)
	{
		command.Id = id;
		await mediator.Send(command);

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete([FromRoute] int id)
	{
		await mediator.Send(new DeleteJobCommand(id));

		return NoContent();
	}


	[HttpGet]
	public async Task<IActionResult> GetAll([FromQuery] GetAllJobsQuery query)
	{
		var jobs = await mediator.Send(query);
		return Ok(jobs);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById([FromRoute] int id)
	{
		return Ok(await mediator.Send(new GetJobByIdQuery(id)));
	}
}
