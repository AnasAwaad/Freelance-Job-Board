using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.RejectOtherProposals;

public class RejectOtherProposalsCommand : IRequest
{
    public int JobId { get; set; }
    public int AcceptedProposalId { get; set; }
}