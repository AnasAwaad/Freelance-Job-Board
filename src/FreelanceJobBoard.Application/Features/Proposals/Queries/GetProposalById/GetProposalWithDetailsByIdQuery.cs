using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalById;
public class GetProposalWithDetailsByIdQuery(int proposalId) : IRequest<ProposalWithDetailsDto>
{
	public int ProposalId { get; } = proposalId;
}
