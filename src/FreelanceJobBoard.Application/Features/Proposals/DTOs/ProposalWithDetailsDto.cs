namespace FreelanceJobBoard.Application.Features.Proposals.DTOs;
public class ProposalWithDetailsDto
{
	public int Id { get; set; }
	public int JobId { get; set; }
	public string? JobTitle { get; set; }
	public string? JobDescription { get; set; }
	public DateTime JobDeadline { get; set; }
	public string JobStatus { get; set; }
	public string ClientName { get; set; } = null!;
	public string? CompanyName { get; set; }
	public string? CoverLetter { get; set; }
	public decimal BidAmount { get; set; }
	public int EstimatedTimelineDays { get; set; }
	public string? Status { get; set; }
	public DateTime? ReviewedAt { get; set; }
	public int? ReviewedBy { get; set; }
	public string? ClientFeedback { get; set; }
	public List<AttachmentDto> Attachments { get; set; } = new();
}
