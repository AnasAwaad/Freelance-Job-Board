using FreelanceJobBoard.Application.Features.Categories.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Queries.GetCategoryById;
public class GetCategoryByIdQuery(int id) : IRequest<CategoryDto>
{
	public int Id { get; } = id;
}
