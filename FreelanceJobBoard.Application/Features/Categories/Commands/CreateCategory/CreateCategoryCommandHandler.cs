using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
internal class CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateCategoryCommand, int>
{
	public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
	{
		var category = mapper.Map<Category>(request);

		await unitOfWork.Categories.CreateAsync(category);
		await unitOfWork.SaveChangesAsync();

		return category.Id;
	}
}
