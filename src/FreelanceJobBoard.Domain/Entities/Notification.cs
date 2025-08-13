using FreelanceJobBoard.Domain.Common;
using FreelanceJobBoard.Domain.Identity;

namespace FreelanceJobBoard.Domain.Entities;
public class Notification : BaseEntity
{
	public int Id { get; set; }
	public int? TemplateId { get; set; } // Made nullable to avoid FK constraint issues
	public string Title { get; set; } = null!;
	public string Message { get; set; } = null!;
	public bool IsRead { get; set; }
	public DateTime? ReadAt { get; set; }
	public int? NotificationTemplateId { get; set; } // Made nullable to avoid FK constraint issues
	
	// Enhanced tracking fields for user interactions
	public string RecipientUserId { get; set; } = null!;
	public string? SenderUserId { get; set; }
	public string Type { get; set; } = null!; // e.g., proposal_received, job_posted, contract_update, etc.
	
	// Related entity references for context
	public int? JobId { get; set; }
	public int? ProposalId { get; set; }
	public int? ContractId { get; set; }
	public int? ReviewId { get; set; }
	
	// Display properties for UI
	public string? Icon { get; set; }
	public string? Color { get; set; }
	public bool IsUrgent { get; set; }
	
	// Additional metadata
	public DateTime? ExpiryDate { get; set; } // For time-sensitive notifications
	public bool IsEmailSent { get; set; } // Track if email was sent
	public string? ActionUrl { get; set; } // Deep link for notification action
	public string? Data { get; set; } // JSON data for additional context

	// Navigation properties
	public string UserId { get; set; } = null!; // Backward compatibility
	public ApplicationUser User { get; set; } = null!;
	public ApplicationUser RecipientUser { get; set; } = null!;
	public ApplicationUser? SenderUser { get; set; }
	public NotificationTemplate? Template { get; set; } // Made nullable since TemplateId is nullable
	
	// Related entity navigation properties
	public Job? Job { get; set; }
	public Proposal? Proposal { get; set; }
	public Contract? Contract { get; set; }
	public Review? Review { get; set; }
}