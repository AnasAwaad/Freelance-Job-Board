using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
	public CategoryRepository(ApplicationDbContext context) : base(context)
	{
	}
}
