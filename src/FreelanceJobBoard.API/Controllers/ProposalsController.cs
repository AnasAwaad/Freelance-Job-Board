using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using FreelanceJobBoard.Application.Features.Proposals.Commands.DeleteFreelancerProposal;
using FreelanceJobBoard.Application.Features.Proposals.Commands.RejectOtherProposals;
using FreelanceJobBoard.Application.Features.Proposals.Commands.UpdateProposalStatus;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposals;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalById;
using FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalsForJob;
using FreelanceJobBoard.Application.Features.Proposals.Queries.HasFreelancerApplied;
using FreelanceJobBoard.Application.Features.Proposals.Queries.HasJobAcceptedProposal;
using FreelanceJobBoard.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProposalsController(IMediator mediator) : ControllerBase
{
	[HttpPost("{jobId}")]
	[Authorize(Roles = AppRoles.Freelancer)]
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

	[HttpGet("job/{jobId}")]
	[Authorize(Roles = AppRoles.Client)]

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
	[Authorize(Roles = AppRoles.Client)]

	public async Task<IActionResult> UpdateProposalStatus(int proposalId, [FromBody] UpdateProposalStatusCommand command)
	{
		command.ProposalId = proposalId;
		await mediator.Send(command);
		return NoContent();
	}

	[HttpGet("freelancer")]
	[Authorize(Roles = AppRoles.Freelancer)]

	public async Task<IActionResult> GetAllProposalsForFreelancer()
	{
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

		var result = await mediator.Send(new GetProposalsForFreelancerQuery(userId));
		return Ok(result);
	}

	[HttpGet("{proposalId}")]
	[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> GetProposalById([FromRoute] int proposalId)
	{
		var result = await mediator.Send(new GetProposalWithDetailsByIdQuery(proposalId));
		return Ok(result);
	}

	[HttpDelete("{proposalId}")]
	[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> DeleteProposalForFreelancer([FromRoute] int proposalId)
	{
		await mediator.Send(new DeleteProposalForFreelancerCommand(proposalId));

		return Ok();
	}

	[HttpGet("freelancer/applied/{jobId}")]
	[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> HasFreelancerApplied([FromRoute] int jobId)
	{
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
		var query = new HasFreelancerAppliedQuery(jobId, userId);
		var result = await mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("job/{jobId}/has-accepted")]
	[Authorize]
	public async Task<IActionResult> HasJobAcceptedProposal([FromRoute] int jobId)
	{
		var query = new HasJobAcceptedProposalQuery(jobId);
		var result = await mediator.Send(query);
		return Ok(result);
	}

	[HttpPost("reject-others")]
	[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> RejectOtherProposals([FromBody] RejectOtherProposalsCommand command)
	{
		await mediator.Send(command);
		return Ok();
	}
}
