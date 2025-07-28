using FreelanceJobBoard.Application.Features.Admin.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Admin.Queries.GetJobWithDetails;
public class GetJobDetailsWithHistoryQuery(int id) : IRequest<JobDetailsDto>
{
	public int Id { get; } = id;
}
