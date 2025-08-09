using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetRecentJobs;
internal class GetRecentJobsQueryHandler(IUnitOfWork unitOfWork,
	IMapper mapper) : IRequestHandler<GetRecentJobsQuery, IEnumerable<RecentJobDto>>
{
	public async Task<IEnumerable<RecentJobDto>> Handle(GetRecentJobsQuery request, CancellationToken cancellationToken)
	{
		var query = unitOfWork.Jobs.GetRecentJobsQueryable(request.NumOfJobs);

		return await mapper.ProjectTo<RecentJobDto>(query).ToListAsync();
	}
}
