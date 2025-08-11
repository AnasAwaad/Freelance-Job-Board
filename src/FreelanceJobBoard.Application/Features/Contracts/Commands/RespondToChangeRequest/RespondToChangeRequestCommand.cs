using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.RespondToChangeRequest;

public class RespondToChangeRequestCommand : IRequest
{
    public int ChangeRequestId { get; set; }
    public bool IsApproved { get; set; }
    public string? ResponseNotes { get; set; }
}