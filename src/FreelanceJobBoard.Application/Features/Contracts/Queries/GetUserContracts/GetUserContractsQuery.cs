using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetUserContracts;

public class GetUserContractsQuery : IRequest<GetUserContractsResult>
{
    public string UserId { get; set; } = string.Empty;
}