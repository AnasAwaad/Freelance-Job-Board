using AutoMapper;
using FreelanceJobBoard.Application.Common;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetAllJobs;
public class GetAllJobsQueryHandler(IMapper mapper, IUnitOfWork unitOfWork) : IRequestHandler<GetAllJobsQuery, PagedResult<JobDto>>
{

	public async Task<PagedResult<JobDto>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
	{

		var (totalCount, jobs) = await unitOfWork.Jobs.GetAllMatchingAsync(request.PageNumber,
																				request.PageSize,
																				request.Search,
																				request.SortBy,
																				request.SortDirection);

		var jobDtos = mapper.Map<IEnumerable<JobDto>>(jobs);


		return new PagedResult<JobDto>(jobDtos, totalCount, request.PageNumber, request.PageSize);


	}
}
