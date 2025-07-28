using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposal;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProposalsController(IMediator mediator) : ControllerBase
{

	[HttpPost("{jobId}")]
	public async Task<IActionResult> SubmitProposal(int jobId, [FromForm] SubmitProposalDto dto)
	{

		//TODO: Replace this with the actual authenticated freelancer ID when auth is added

		var command = new CreateProposalCommand
		{
			JobId = jobId,
			FreelancerId = 6,
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
