using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.UpdateCategory;
internal class UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateCategoryCommand>
{
	public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
	{
		var category = await unitOfWork.Categories.GetByIdAsync(request.Id);

		if (category is null)
			throw new NotFoundException(nameof(Category), request.Id.ToString());

		mapper.Map(request, category);

		await unitOfWork.SaveChangesAsync();
	}
}
