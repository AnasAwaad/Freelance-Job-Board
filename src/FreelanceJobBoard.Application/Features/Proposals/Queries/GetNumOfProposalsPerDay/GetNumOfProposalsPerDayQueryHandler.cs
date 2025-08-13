using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetNumOfProposalsPerDay;
internal class GetNumOfProposalsPerDayQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetNumOfProposalsPerDayQuery, IEnumerable<ProposalsPerDayResultDto>>
{
	public async Task<IEnumerable<ProposalsPerDayResultDto>> Handle(GetNumOfProposalsPerDayQuery request, CancellationToken cancellationToken)
	{
		return await unitOfWork.Proposals.GetNumOfProposalsPerDayAsync();
	}
}
