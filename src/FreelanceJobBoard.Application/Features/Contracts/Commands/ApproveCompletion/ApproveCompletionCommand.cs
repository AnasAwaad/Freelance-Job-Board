using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.ApproveCompletion;

public class ApproveCompletionCommand : IRequest
{
    public int ContractId { get; set; }
    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
}