using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;
public interface ICategoryRepository : IGenericRepository<Category>
{
	Task<List<Category>> GetCategoriesByIdsAsync(IEnumerable<int> categoryIds);
}
