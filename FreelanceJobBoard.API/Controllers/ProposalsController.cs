using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposal;
using FreelanceJobBoard.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = AppRoles.Freelancer)]
public class ProposalsController(IMediator mediator) : ControllerBase
{

	[HttpPost("{jobId}")]
	public async Task<IActionResult> SubmitProposal(int jobId, [FromForm] SubmitProposalDto dto)
	{

		var command = new CreateProposalCommand
		{
			JobId = jobId,
			UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!,
			CoverLetter = dto.CoverLetter,
			BidAmount = dto.BidAmount,
			EstimatedTimelineDays = dto.EstimatedTimelineDays,
			PortfolioFiles = dto.PortfolioFiles
		};

		await mediator.Send(command);
		return Created();
	}

	[HttpGet("freelancer")]
	public async Task<IActionResult> GetAllProposalsForFreelancer()
	{
		//TODO: Replace this with the actual authenticated freelancer ID when auth is added
		int freelancerId = 6;

		var result = await mediator.Send(new GetProposalsForFreelancerQuery(freelancerId));
		return Ok(result);
	}
}
