using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
public class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand>
{
	public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
	{
		var category = await unitOfWork.Categories.GetByIdAsync(request.Id);

		if (category is null)
			throw new NotFoundException(nameof(Category), request.Id.ToString());

		unitOfWork.Categories.Delete(category);
		await unitOfWork.SaveChangesAsync();
	}
}
