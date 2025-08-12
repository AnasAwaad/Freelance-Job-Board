using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;

public class ContractVersion : BaseEntity
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTime? ProjectDeadline { get; set; }
    public string? Deliverables { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? AdditionalNotes { get; set; }
    
    // Version metadata
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByRole { get; set; } = string.Empty; // "Client" or "Freelancer"
    // Note: CreatedOn is inherited from BaseEntity, no need to redeclare
    public bool IsCurrentVersion { get; set; }
    public string? ChangeReason { get; set; }
    
    // Navigation properties
    public Contract Contract { get; set; } = null!;
}