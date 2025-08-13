using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetRecentJobs;
internal class GetRecentOpenedJobsQueryHandler(IUnitOfWork unitOfWork,
	IMapper mapper) : IRequestHandler<GetRecentOpenedJobsQuery, IEnumerable<RecentJobDto>>
{
	public async Task<IEnumerable<RecentJobDto>> Handle(GetRecentOpenedJobsQuery request, CancellationToken cancellationToken)
	{
		var query = unitOfWork.Jobs.GetRecentOpenedJobsQueryable(request.NumOfJobs);

		return await mapper.ProjectTo<RecentJobDto>(query).ToListAsync();
	}
}
