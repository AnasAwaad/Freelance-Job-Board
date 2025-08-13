using FreelanceJobBoard.Application.Features.User.Commands.UpdateProfile;
using FreelanceJobBoard.Application.Features.User.Queries.GetNumberOfClients;
using FreelanceJobBoard.Application.Features.User.Queries.GetNumberOfFreelancers;
using FreelanceJobBoard.Application.Features.User.Queries.GetProfile;
using FreelanceJobBoard.Application.Features.User.Queries.GetTopClients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class UserController : ControllerBase
{
	private readonly IMediator _mediator;
	public UserController(IMediator mediator)
	{
		_mediator = mediator;
	}
	[HttpGet("get-profile")]
	public async Task<IActionResult> GetProfile()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var query = new GetProfileQuery { UserId = userId };
		var profile = await _mediator.Send(query);
		return Ok(profile);
	}

	[HttpPut("update-freelancer-profile")]
	public async Task<IActionResult> UpdateFreelancerProfile([FromForm] UpdateFreelancerProfileCommand command)
	{
		await _mediator.Send(command);
		return Ok(new { success = true, message = "Freelancer profile updated successfully" });
	}

	[HttpPut("update-client-profile")]
	public async Task<IActionResult> UpdateClientProfile([FromForm] UpdateClientProfileCommand command)
	{
		await _mediator.Send(command);
		return Ok(new { success = true, message = "Client profile updated successfully" });
	}

	[HttpGet("total-clients")]
	public async Task<IActionResult> GetNumOfClients()
	{
		var result = await _mediator.Send(new GetNumberOfClientsQuery());
		return Ok(result);
	}


	[HttpGet("total-freelancers")]
	public async Task<IActionResult> GetNumOfFreelancers()
	{
		var result = await _mediator.Send(new GetNumberOfFreelancersQuery());
		return Ok(result);
	}

	[HttpGet("top-clients/{numOfClients}")]
	public async Task<IActionResult> GetTopClients(int numOfClients)
	{
		var result = await _mediator.Send(new GetTopClientsQuery(numOfClients));
		return Ok(result);
	}

}
