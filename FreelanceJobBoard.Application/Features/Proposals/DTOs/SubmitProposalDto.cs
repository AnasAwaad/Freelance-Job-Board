using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Application.Features.Proposals.DTOs;
public class SubmitProposalDto
{
	public string? CoverLetter { get; set; }
	public decimal BidAmount { get; set; }
	public int EstimatedTimelineDays { get; set; }
	public List<IFormFile>? PortfolioFiles { get; set; }
}
