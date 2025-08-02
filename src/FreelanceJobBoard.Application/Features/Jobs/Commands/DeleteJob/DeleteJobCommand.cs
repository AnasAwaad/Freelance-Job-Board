using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.DeleteJob;
public class DeleteJobCommand(int id) : IRequest
{
	public int Id { get; } = id;
}
