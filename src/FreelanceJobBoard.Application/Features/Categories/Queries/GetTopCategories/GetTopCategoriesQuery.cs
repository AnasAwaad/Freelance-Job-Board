using FreelanceJobBoard.Application.Features.Categories.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Queries.GetTopCategories;
public class GetTopCategoriesQuery(int numOfCategories) : IRequest<IEnumerable<PublicCategoryDto>>
{
	public int NumOfCategories { get; } = numOfCategories;
}
