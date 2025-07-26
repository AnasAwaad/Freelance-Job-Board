using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
public class CreateProposalCommand : IRequest
{
	[BindNever]
	public int JobId { get; set; }
	[BindNever]
	public int FreelancerId { get; set; }
	public string? CoverLetter { get; set; }
	public decimal BidAmount { get; set; }
	public int EstimatedTimelineDays { get; set; }
	[FromForm]
	public IFormFileCollection? PortfolioFiles { get; set; }
}
