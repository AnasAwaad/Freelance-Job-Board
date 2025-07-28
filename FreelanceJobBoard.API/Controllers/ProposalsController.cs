using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using FreelanceJobBoard.Application.Features.Proposals.Commands.UpdateProposalStatus;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposal;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalsForJob;
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
		var command = new CreateProposalCommand
		{
			JobId = jobId,
			CoverLetter = dto.CoverLetter,
			BidAmount = dto.BidAmount,
			EstimatedTimelineDays = dto.EstimatedTimelineDays,
			PortfolioFiles = dto.PortfolioFiles
		};

		await mediator.Send(command);
		return Created();
	}

	[HttpGet("job/{jobId}")]
	public async Task<IActionResult> GetProposalsForJob(int jobId, [FromQuery] string? status = null)
	{
		var query = new GetProposalsForJobQuery
		{
			JobId = jobId,
			Status = status
		};

		var result = await mediator.Send(query);
		return Ok(result);
	}

	[HttpPut("{proposalId}/status")]
	public async Task<IActionResult> UpdateProposalStatus(int proposalId, [FromBody] UpdateProposalStatusCommand command)
	{
		command.ProposalId = proposalId;
		await mediator.Send(command);
		return NoContent();
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
