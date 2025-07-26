using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class JobRepository : GenericRepository<Job>, IJobRepository
{
	public JobRepository(ApplicationDbContext context) : base(context)
	{
	}

	public async Task<Job?> GetJobWithCategoriesAndSkillsAsync(int id)
	{
		return await _context.Jobs
			.Include(j => j.Skills)
			.Include(j => j.Categories)
			.FirstOrDefaultAsync(j => j.Id == id);
	}
}
