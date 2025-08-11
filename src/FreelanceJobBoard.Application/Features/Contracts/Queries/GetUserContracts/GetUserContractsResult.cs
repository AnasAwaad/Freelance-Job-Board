using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetUserContracts;

public class GetUserContractsResult
{
    public IEnumerable<ContractDto> Contracts { get; set; } = new List<ContractDto>();
}

public class ContractDto
{
    public int Id { get; set; }
    public int ProposalId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public string? AgreedPaymentType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ContractStatus { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime? LastUpdatedOn { get; set; }
}