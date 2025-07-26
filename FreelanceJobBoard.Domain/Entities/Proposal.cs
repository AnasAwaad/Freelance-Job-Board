using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Proposal : BaseEntity
{
	public int Id { get; set; }
	public int JobId { get; set; }
	public int? ClientId { get; set; }
	public int? FreelancerId { get; set; }
	public string? CoverLetter { get; set; }
	public decimal BidAmount { get; set; }
	public int EstimatedTimelineDays { get; set; }
	public string? Status { get; set; }
	public DateTime? ReviewedAt { get; set; }
	public int? ReviewedBy { get; set; }
	public string? ClientFeedback { get; set; }

	public Job Job { get; set; }
	public Client? Client { get; set; }
	public Freelancer? Freelancer { get; set; }
	public Contract? Contract { get; set; }

	public ICollection<ProposalAttachment> Attachments { get; set; }
}

