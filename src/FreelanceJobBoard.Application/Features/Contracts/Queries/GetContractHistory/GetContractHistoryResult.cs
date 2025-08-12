namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractHistory;

public class GetContractHistoryResult
{
    public int ContractId { get; set; }
    public ContractVersionDto CurrentVersion { get; set; } = null!;
    public IEnumerable<ContractVersionDto> VersionHistory { get; set; } = new List<ContractVersionDto>();
    public IEnumerable<ContractChangeRequestDto> ChangeRequests { get; set; } = new List<ContractChangeRequestDto>();
}

public class ContractVersionDto
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTime? ProjectDeadline { get; set; }
    public string? Deliverables { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? AdditionalNotes { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByRole { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public bool IsCurrentVersion { get; set; }
    public string? ChangeReason { get; set; }
}

public class ContractChangeRequestDto
{
    public int Id { get; set; }
    public string RequestedByRole { get; set; } = string.Empty;
    public string ChangeDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public string? ResponseByRole { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? ResponseNotes { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public ContractVersionDto FromVersion { get; set; } = null!;
    public ContractVersionDto ProposedVersion { get; set; } = null!;
}