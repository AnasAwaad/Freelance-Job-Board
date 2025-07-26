using FreelanceJobBoard.Application.Common;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Domain.Constants;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetAllJobs;
public class GetAllJobsQuery : IRequest<PagedResult<JobDto>>
{
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public string? Search { get; set; }
	public string? SortBy { get; set; }
	public SortDirection SortDirection { get; set; }
}
