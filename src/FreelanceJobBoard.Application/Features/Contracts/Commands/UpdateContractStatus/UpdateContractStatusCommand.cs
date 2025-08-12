using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.UpdateContractStatus;

public class UpdateContractStatusCommand : IRequest
{
    public int ContractId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}