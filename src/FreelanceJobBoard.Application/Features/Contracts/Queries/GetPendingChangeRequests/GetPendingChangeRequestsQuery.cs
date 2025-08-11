using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetPendingChangeRequests;

public class GetPendingChangeRequestsQuery : IRequest<GetPendingChangeRequestsResult>
{
    public string? UserId { get; set; }
}