using AutoMapper;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
public class CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
	public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
	{
		var category = mapper.Map<Category>(request);

		await unitOfWork.Categories.CreateAsync(category);
		await unitOfWork.SaveChangesAsync();

		return mapper.Map<CategoryDto>(category);
	}
}
