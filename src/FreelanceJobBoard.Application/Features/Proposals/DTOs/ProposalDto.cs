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
	
	// Job related information
	public string? JobTitle { get; set; }
	public string? JobDescription { get; set; }
	public decimal JobBudgetMin { get; set; }
	public decimal JobBudgetMax { get; set; }
	public DateTime JobDeadline { get; set; }
	
	// Client information (for freelancer viewing their proposals)
	public string? ClientName { get; set; }
	public string? ClientProfileImageUrl { get; set; }
	public decimal ClientAverageRating { get; set; }
	public int ClientTotalReviews { get; set; }
	
	// Freelancer information (for client viewing job proposals)
	public string? FreelancerName { get; set; }
	public string? FreelancerProfileImageUrl { get; set; }
	public decimal FreelancerAverageRating { get; set; }
	public int FreelancerTotalReviews { get; set; }
}
