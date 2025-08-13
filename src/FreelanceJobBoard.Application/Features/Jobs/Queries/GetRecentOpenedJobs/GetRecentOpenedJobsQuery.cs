using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetRecentJobs;
public class GetRecentOpenedJobsQuery(int numOfJobs) : IRequest<IEnumerable<RecentJobDto>>
{
	public int NumOfJobs { get; } = numOfJobs;
}
