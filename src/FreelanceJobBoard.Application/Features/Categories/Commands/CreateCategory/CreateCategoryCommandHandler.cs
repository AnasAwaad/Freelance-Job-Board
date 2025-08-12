using AutoMapper;
using FreelanceJobBoard.Application.Features.Categories.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
public class CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateCategoryCommandHandler> logger) : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
	public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
	{
		logger.LogInformation("🆕 Starting category creation | CategoryName={CategoryName}", request.Name);

		try
		{
			var category = mapper.Map<Category>(request);

			logger.LogDebug("💾 Saving category to database | CategoryName={CategoryName}, Description={Description}", 
				request.Name, request.Description ?? "No description");

			await unitOfWork.Categories.CreateAsync(category);
			await unitOfWork.SaveChangesAsync();

			var result = mapper.Map<CategoryDto>(category);

			logger.LogInformation("✅ Category created successfully | CategoryId={CategoryId}, CategoryName={CategoryName}", 
				category.Id, request.Name);

			return result;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "❌ Failed to create category | CategoryName={CategoryName}", request.Name);
			throw;
		}
	}
}
