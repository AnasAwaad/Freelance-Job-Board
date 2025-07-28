using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
public class GetJobByIdQuery(int id) : IRequest<JobDto>
{
	public int Id { get; } = id;
}
