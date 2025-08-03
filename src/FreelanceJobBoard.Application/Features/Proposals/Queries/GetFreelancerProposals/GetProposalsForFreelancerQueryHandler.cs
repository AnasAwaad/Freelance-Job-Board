using AutoMapper;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetFreelancerProposals;
public class GetProposalsForFreelancerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
	: IRequestHandler<GetProposalsForFreelancerQuery, IEnumerable<ProposalDto>>
{
	public async Task<IEnumerable<ProposalDto>> Handle(GetProposalsForFreelancerQuery request, CancellationToken cancellationToken)
	{
		var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(request.UserId);

		if (freelancer is null)
			throw new UnauthorizedAccessException();

		var proposals = await unitOfWork.Proposals.GetAllByFreelancerIdAsync(freelancer.Id);

		return mapper.Map<IEnumerable<ProposalDto>>(proposals);
	}
}
