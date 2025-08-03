using AutoMapper;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalById;
public class GetProposalWithDetailsByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
	: IRequestHandler<GetProposalWithDetailsByIdQuery, ProposalWithDetailsDto>
{
	public async Task<ProposalWithDetailsDto> Handle(GetProposalWithDetailsByIdQuery request, CancellationToken cancellationToken)
	{
		var proposalQuerable = unitOfWork.Proposals.GetByIdWithDetailsQueryable(request.ProposalId);

		var proposal = await mapper
			.ProjectTo<ProposalWithDetailsDto>(proposalQuerable)
			.FirstOrDefaultAsync();

		if (proposal is null)
			throw new NotFoundException(nameof(Proposal), request.ProposalId.ToString());

		return proposal;
	}
}
