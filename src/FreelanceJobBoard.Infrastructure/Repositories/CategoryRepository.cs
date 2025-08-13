using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
	public CategoryRepository(ApplicationDbContext context, ILogger<GenericRepository<Category>>? logger = null) : base(context, logger)
	{
	}

	public async Task<List<Category>> GetCategoriesByIdsAsync(IEnumerable<int> categoryIds)
	{
		_logger?.LogDebug("🔍 Getting categories by IDs | CategoryIds={CategoryIds}", string.Join(",", categoryIds));
		
		try
		{
			var categories = await _context.Categories
				.Where(c => categoryIds.Contains(c.Id))
				.ToListAsync();

			_logger?.LogDebug("✅ Categories retrieved | RequestedCount={RequestedCount}, FoundCount={FoundCount}", 
				categoryIds.Count(), categories.Count);
			
			return categories;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to get categories by IDs | CategoryIds={CategoryIds}", string.Join(",", categoryIds));
			throw;
		}
	}

	public async Task<IEnumerable<Category>> GetTopCategoriesAsync(int numOfCategories)
	{
		return await _context.Categories
			.Include(c => c.JobCategories)
			.OrderByDescending(c => c.JobCategories.Count)
			.Take(numOfCategories)
			.ToListAsync();
	}
}
