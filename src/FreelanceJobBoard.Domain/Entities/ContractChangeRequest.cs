using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;

public class ContractChangeRequest : BaseEntity
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int FromVersionId { get; set; }
    public int ProposedVersionId { get; set; }
    
    // Request details
    public string RequestedByUserId { get; set; } = string.Empty;
    public string RequestedByRole { get; set; } = string.Empty; // "Client" or "Freelancer"
    public string ChangeDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Pending", "Approved", "Rejected"
    
    // Response details
    public string? ResponseByUserId { get; set; }
    public string? ResponseByRole { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? ResponseNotes { get; set; }
    
    // Metadata
    public DateTime RequestDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    
    // Navigation properties
    public Contract Contract { get; set; } = null!;
    public ContractVersion FromVersion { get; set; } = null!;
    public ContractVersion ProposedVersion { get; set; } = null!;
}