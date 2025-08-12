using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.RequestCompletion;

public class RequestCompletionCommand : IRequest
{
    public int ContractId { get; set; }
    public string? Notes { get; set; }
}