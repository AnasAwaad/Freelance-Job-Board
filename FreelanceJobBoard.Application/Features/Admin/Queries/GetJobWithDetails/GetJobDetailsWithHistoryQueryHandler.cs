using AutoMapper;
using FreelanceJobBoard.Application.Features.Admin.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Features.Admin.Queries.GetJobWithDetails;
internal class GetJobDetailsWithHistoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetJobDetailsWithHistoryQuery, JobDetailsDto>
{
	public async Task<JobDetailsDto> Handle(GetJobDetailsWithHistoryQuery request, CancellationToken cancellationToken)
	{
		var jobQuery = unitOfWork.Jobs.GetJobWithProposalsAndReviewQuery(request.Id);

		var job = await mapper.ProjectTo<JobDetailsDto>(jobQuery).FirstOrDefaultAsync();

		if (job is null)
			throw new NotFoundException(nameof(Job), request.Id.ToString());

		return job;
	}
}
