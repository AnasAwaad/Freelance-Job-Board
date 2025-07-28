using FreelanceJobBoard.Application.Features.Admin.DTOs;
using FreelanceJobBoard.Domain.Constants;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Admin.Queries.GetAllJobs;
public class GetAllJobsWithStatusQuery(JobStatus? status) : IRequest<IEnumerable<JobListDto>>
{
	public JobStatus? Status { get; } = status;
}
