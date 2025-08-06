using FreelanceJobBoard.Application.Features.Categories.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
public class ChangeCategoryStatusCommand(int id) : IRequest<ChangeCategoryStatusResultDto>
{
	public int Id { get; } = id;
}
