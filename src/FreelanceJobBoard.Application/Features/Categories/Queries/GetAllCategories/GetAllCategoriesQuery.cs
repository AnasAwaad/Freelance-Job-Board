using FreelanceJobBoard.Application.Features.Categories.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Queries.GetAllCategories;
public class GetAllCategoriesQuery : IRequest<IEnumerable<CategoryDto>>
{
}
