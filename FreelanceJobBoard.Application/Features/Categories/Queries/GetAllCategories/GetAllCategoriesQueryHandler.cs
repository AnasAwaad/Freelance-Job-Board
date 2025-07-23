using AutoMapper;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Queries.GetAllCategories;
public class GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
{

	async Task<IEnumerable<CategoryDto>> IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>.Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
	{
		var categories = await unitOfWork.Categories.GetAllAsync();

		return mapper.Map<IEnumerable<CategoryDto>>(categories);
	}
}
