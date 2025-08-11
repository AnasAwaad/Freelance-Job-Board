using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractDetails;

public class GetContractDetailsQuery : IRequest<ContractDetailsDto>
{
    public int ContractId { get; set; }
}