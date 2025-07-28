using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.ApproveJob;

public class ApproveJobCommand : IRequest
{
    public int JobId { get; set; }
    public bool IsApproved { get; set; }
    public string? AdminMessage { get; set; }
    public string AdminUserId { get; set; } = null!;
}