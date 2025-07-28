using AutoMapper;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposal;
internal class GetProposalsForFreelancerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
	: IRequestHandler<GetProposalsForFreelancerQuery, IEnumerable<ProposalDto>>
{
	public async Task<IEnumerable<ProposalDto>> Handle(GetProposalsForFreelancerQuery request, CancellationToken cancellationToken)
	{
		var proposals = await unitOfWork.Proposals.GetAllByFreelancerIdAsync(request.freelancerId);

		return mapper.Map<IEnumerable<ProposalDto>>(proposals);
	}
}
