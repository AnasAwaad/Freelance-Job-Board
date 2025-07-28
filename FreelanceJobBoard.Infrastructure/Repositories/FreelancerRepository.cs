using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class FreelancerRepository : GenericRepository<Freelancer>, IFreelancerRepository
{
	public FreelancerRepository(ApplicationDbContext context) : base(context)
	{
	}

	public async Task<Freelancer?> GetByUserIdAsync(string userId)
	{
		return await _context.Freelancers
			.Where(c => c.UserId == userId)
			.FirstOrDefaultAsync();
	}
}
