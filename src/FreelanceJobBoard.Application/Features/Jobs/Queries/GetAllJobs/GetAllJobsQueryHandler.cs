using AutoMapper;
using FreelanceJobBoard.Application.Common;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetAllJobs;
public class GetAllJobsQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<GetAllJobsQuery, PagedResult<JobDto>>
{

	public async Task<PagedResult<JobDto>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
	{
		string? statusFilter = null;

		// For now, we'll apply a simple approach:
		// - If user is authenticated, check if they're a freelancer by trying to find their freelancer record
		// - If not authenticated, only show open jobs
		if (currentUserService.IsAuthenticated && !string.IsNullOrEmpty(currentUserService.UserId))
		{
			// Check if user is a freelancer by looking for freelancer record
			var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId);
			if (freelancer != null)
			{
				// User is a freelancer, only show open jobs
				statusFilter = JobStatus.Open;
			}
			// If not a freelancer, assume they are admin/client and can see all jobs
		}
		else
		{
			// For unauthenticated users, only show open jobs
			statusFilter = JobStatus.Open;
		}

		var (totalCount, jobs) = await unitOfWork.Jobs.GetAllMatchingAsync(
			request.PageNumber,
			request.PageSize,
			request.Search,
			request.SortBy,
			request.SortDirection,
			request.Category,
			statusFilter);

		var jobDtos = mapper.Map<IEnumerable<JobDto>>(jobs);

		return new PagedResult<JobDto>(jobDtos, totalCount, request.PageNumber, request.PageSize);
	}
}
