using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
public class ChangeCategoryStatusCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeCategoryStatusCommand, ChangeCategoryStatusResultDto>
{
	public async Task<ChangeCategoryStatusResultDto> Handle(ChangeCategoryStatusCommand request, CancellationToken cancellationToken)
	{
		var category = await unitOfWork.Categories.GetByIdAsync(request.Id);

		if (category is null)
			throw new NotFoundException(nameof(Category), request.Id.ToString());
		category.IsActive = !category.IsActive;
		await unitOfWork.SaveChangesAsync();

		return new ChangeCategoryStatusResultDto
		{
			IsActive = category.IsActive,
			LastUpdatedOn = DateTime.UtcNow,
		};
	}
}
