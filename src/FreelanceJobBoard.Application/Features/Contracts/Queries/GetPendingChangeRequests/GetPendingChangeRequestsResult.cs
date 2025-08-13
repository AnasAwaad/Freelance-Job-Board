using FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractHistory;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetPendingChangeRequests;

public class GetPendingChangeRequestsResult
{
    public IEnumerable<PendingChangeRequestDto> PendingRequests { get; set; } = new List<PendingChangeRequestDto>();
}

public class PendingChangeRequestDto : ContractChangeRequestDto
{
    public string ContractJobTitle { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public bool RequiresUserResponse { get; set; }
}