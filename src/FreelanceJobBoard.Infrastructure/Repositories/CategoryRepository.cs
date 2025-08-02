using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
	public CategoryRepository(ApplicationDbContext context) : base(context)
	{
	}

	public async Task<List<Category>> GetCategoriesByIdsAsync(IEnumerable<int> categoryIds)
	{
		return await _context.Categories
			.Where(c => categoryIds.Contains(c.Id))
			.ToListAsync();
	}
}
