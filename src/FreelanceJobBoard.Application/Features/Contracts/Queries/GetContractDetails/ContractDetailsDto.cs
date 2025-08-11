namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractDetails;

public class ContractDetailsDto
{
    public int Id { get; set; }
    public int ProposalId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public string FreelancerEmail { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public string? AgreedPaymentType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ContractStatus { get; set; } = string.Empty;
    public int ContractStatusId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastUpdatedOn { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public int EstimatedTimelineDays { get; set; }
    
    // Version-specific fields
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ProjectDeadline { get; set; }
    public string? Deliverables { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? AdditionalNotes { get; set; }
    public int CurrentVersionNumber { get; set; }
    public DateTime LastVersionUpdateDate { get; set; }
}