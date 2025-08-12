using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractHistory;

public class GetContractHistoryQuery : IRequest<GetContractHistoryResult>
{
    public int ContractId { get; set; }
}