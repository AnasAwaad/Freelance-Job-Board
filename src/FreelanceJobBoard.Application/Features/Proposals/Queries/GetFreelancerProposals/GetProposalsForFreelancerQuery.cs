using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposals;
public class GetProposalsForFreelancerQuery(string userId) : IRequest<IEnumerable<ProposalDto>>
{
	public string UserId { get; } = userId;
}
