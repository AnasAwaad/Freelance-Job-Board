using AutoMapper;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Queries.GetTopCategories;
internal class GetTopCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTopCategoriesQuery, IEnumerable<PublicCategoryDto>>
{
	public async Task<IEnumerable<PublicCategoryDto>> Handle(GetTopCategoriesQuery request, CancellationToken cancellationToken)
	{
		var categories = await unitOfWork.Categories.GetTopCategoriesAsync(request.NumOfCategories);

		return mapper.Map<IEnumerable<PublicCategoryDto>>(categories);
	}
}
