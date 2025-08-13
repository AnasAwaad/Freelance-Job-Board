using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetRelatedJobs;
public class GetRetatedJobsForJobQuery(int jobId) : IRequest<IEnumerable<PublicJobListDto>>
{
	public int Id { get; } = jobId;
}