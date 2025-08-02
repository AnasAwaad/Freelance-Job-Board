namespace FreelanceJobBoard.Application.Features.Proposals.DTOs;
public class ProposalDto
{
	public int Id { get; set; }
	public int JobId { get; set; }
	public int? ClientId { get; set; }
	public string? CoverLetter { get; set; }
	public decimal BidAmount { get; set; }
	public int EstimatedTimelineDays { get; set; }
	public string? Status { get; set; }
	public DateTime? ReviewedAt { get; set; }
	public int? ReviewedBy { get; set; }
	public string? ClientFeedback { get; set; }
	public List<AttachmentDto> Attachments { get; set; } = new();

}
