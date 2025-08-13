using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
public class DeleteCategoryCommand(int id) : IRequest<bool>
{
	public int Id { get; } = id;
}