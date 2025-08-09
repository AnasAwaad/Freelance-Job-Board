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
    public async Task<Freelancer?> GetByUserIdWithDetailsAsync(string userId)
    {
        return await _context.Freelancers
            .Include(f => f.FreelancerSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(f => f.Certifications)
            .Where(f => f.UserId == userId)
            .FirstOrDefaultAsync();
    }
}
