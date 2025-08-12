using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.CompleteJob;

public class CompleteJobCommand : IRequest<bool>
{
    public int JobId { get; set; }
    public string? CompletionNotes { get; set; }
    public List<string>? DeliverableUrls { get; set; }
}