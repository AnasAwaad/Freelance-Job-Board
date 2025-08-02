using FreelanceJobBoard.Application.Features.Admin.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
public class GetJobByIdQuery(int id) : IRequest<JobDetailsDto>
{
	public int Id { get; } = id;
}
