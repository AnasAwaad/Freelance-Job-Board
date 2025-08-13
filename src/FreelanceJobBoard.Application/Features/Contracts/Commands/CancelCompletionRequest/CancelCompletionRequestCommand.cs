using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.CancelCompletionRequest;

public class CancelCompletionRequestCommand : IRequest
{
    public int ContractId { get; set; }
    public string? Notes { get; set; }
}