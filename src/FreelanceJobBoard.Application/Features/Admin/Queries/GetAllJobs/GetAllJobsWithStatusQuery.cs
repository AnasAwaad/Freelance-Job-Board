using FreelanceJobBoard.Application.Features.Admin.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Admin.Queries.GetAllJobs;
public class GetAllJobsWithStatusQuery(string? status) : IRequest<IEnumerable<JobListDto>>
{
	public string? Status { get; } = status;
}
