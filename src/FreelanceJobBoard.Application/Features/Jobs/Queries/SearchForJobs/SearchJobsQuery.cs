using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.SearchForJobs;
public class SearchJobsQuery(string Query, int Limit = 20) : IRequest<IEnumerable<JobSearchDto>>
{
	public int Limit { get; } = Limit;
	public string Query { get; } = Query;

}
