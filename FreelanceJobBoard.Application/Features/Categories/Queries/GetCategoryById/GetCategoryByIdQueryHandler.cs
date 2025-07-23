using AutoMapper;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Queries.GetCategoryById;
public class GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
	public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
	{
		var category = await unitOfWork.Categories.GetByIdAsync(request.Id);

		if (category is null)
			throw new NotFoundException(nameof(Category), request.Id.ToString());

		return mapper.Map<CategoryDto>(category);
	}
}
