using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
public class DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteCategoryCommandHandler> logger) : IRequestHandler<DeleteCategoryCommand, bool>
{
	public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
	{
		try
		{
			logger.LogInformation("??? Starting category deletion | CategoryId={CategoryId}", request.Id);

			var category = await unitOfWork.Categories.GetByIdAsync(request.Id);

			if (category is null)
			{
				logger.LogWarning("? Category not found for deletion | CategoryId={CategoryId}", request.Id);
				throw new NotFoundException(nameof(Category), request.Id.ToString());
			}

			// Check if category has any associated jobs by getting all job categories
			var allJobCategories = await unitOfWork.JobCategories.GetAllAsync();
			var hasJobs = allJobCategories.Any(jc => jc.CategoryId == request.Id);
			
			if (hasJobs)
			{
				logger.LogWarning("?? Cannot delete category with associated jobs | CategoryId={CategoryId}, CategoryName='{CategoryName}'", 
					request.Id, category.Name);
				throw new InvalidOperationException($"Cannot delete category '{category.Name}' because it has associated jobs. Please remove all job associations first.");
			}

			// Perform the actual deletion
			unitOfWork.Categories.Delete(category);
			await unitOfWork.SaveChangesAsync();

			logger.LogInformation("? Category deleted successfully | CategoryId={CategoryId}, CategoryName='{CategoryName}'", 
				request.Id, category.Name);

			return true;
		}
		catch (Exception ex) when (ex is not NotFoundException && ex is not InvalidOperationException)
		{
			logger.LogError(ex, "?? Unexpected error while deleting category | CategoryId={CategoryId}", request.Id);
			throw;
		}
	}
}