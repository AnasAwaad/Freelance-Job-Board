using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

internal class FreelancerRepository : GenericRepository<Freelancer>, IFreelancerRepository
{
	public FreelancerRepository(ApplicationDbContext context, ILogger<GenericRepository<Freelancer>>? logger = null) : base(context, logger)
	{
	}

	public async Task<Freelancer?> GetByUserIdAsync(string userId)
	{
		return await _context.Freelancers
			.Include(f => f.User)
			.FirstOrDefaultAsync(f => f.UserId == userId);
	}

	public async Task<Freelancer?> GetFreelancerWithSkillsAsync(int freelancerId)
	{
		return await _context.Freelancers
			.Include(f => f.FreelancerSkills)
				.ThenInclude(fs => fs.Skill)
			.Include(f => f.User)
			.FirstOrDefaultAsync(f => f.Id == freelancerId);
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

	public async Task<int> GetTotalNumbers()
	{
		return await _context.Freelancers.CountAsync();
	}
}