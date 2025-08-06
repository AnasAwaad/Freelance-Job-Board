using FreelanceJobBoard.Application.Features.Categories.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
public class CreateCategoryCommand : IRequest<CategoryDto>
{
	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;
}
