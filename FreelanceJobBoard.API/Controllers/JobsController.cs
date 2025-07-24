using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
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
}
