using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.UpdateProposalStatus;

public class UpdateProposalStatusCommand : IRequest
{
    public int ProposalId { get; set; }
    public string Status { get; set; } = null!;
    public string? ClientFeedback { get; set; }
}