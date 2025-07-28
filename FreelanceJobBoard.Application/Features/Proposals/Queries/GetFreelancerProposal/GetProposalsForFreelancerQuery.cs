using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposal;
public class GetProposalsForFreelancerQuery(int freelancerId) : IRequest<IEnumerable<ProposalDto>>
{
	public int freelancerId { get; } = freelancerId;
}
