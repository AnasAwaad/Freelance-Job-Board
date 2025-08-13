using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetNumOfProposalsPerDay;
public class GetNumOfProposalsPerDayQuery : IRequest<IEnumerable<ProposalsPerDayResultDto>>
{
}
