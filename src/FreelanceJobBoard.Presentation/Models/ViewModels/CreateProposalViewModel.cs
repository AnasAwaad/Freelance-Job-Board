namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class CreateProposalViewModel
{
	public int JobId { get; set; }
	public string? CoverLetter { get; set; }
	public decimal BidAmount { get; set; }
	public int EstimatedTimelineDays { get; set; }
	public List<IFormFile>? PortfolioFiles { get; set; }
}
