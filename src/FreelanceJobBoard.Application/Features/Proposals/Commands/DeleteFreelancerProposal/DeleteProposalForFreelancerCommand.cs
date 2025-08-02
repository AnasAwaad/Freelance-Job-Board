using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.DeleteFreelancerProposal;
public class DeleteProposalForFreelancerCommand(int proposalId) : IRequest
{
	public int ProposalId { get; } = proposalId;
}
