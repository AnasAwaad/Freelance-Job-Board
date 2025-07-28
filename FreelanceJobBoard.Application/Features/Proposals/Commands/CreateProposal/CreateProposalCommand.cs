using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;

public class CreateProposalCommand : IRequest
{
	[BindNever]
	public int JobId { get; set; }
	
	[FromForm]
	public string? CoverLetter { get; set; }
	
	[FromForm]
	public decimal BidAmount { get; set; }
	
	[FromForm]
	public int EstimatedTimelineDays { get; set; }
	
	[FromForm]
	public List<IFormFile>? PortfolioFiles { get; set; }
}
