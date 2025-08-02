using MediatR;
using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;

public class CreateProposalCommand : IRequest
{
	public int JobId { get; set; }
	public string UserId { get; set; } = null!;
	public string? CoverLetter { get; set; }
	public decimal BidAmount { get; set; }
	public int EstimatedTimelineDays { get; set; }
	public List<IFormFile>? PortfolioFiles { get; set; }
}
