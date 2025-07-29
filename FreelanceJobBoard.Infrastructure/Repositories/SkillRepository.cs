using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class SkillRepository : GenericRepository<Skill>, ISkillRepository

{
	public SkillRepository(ApplicationDbContext context) : base(context)
	{
	}

	public async Task<List<Skill>> GetSkillsByIdsAsync(IEnumerable<int> skillIds)
	{
		return await _context.Skills
			.Where(s => skillIds.Contains(s.Id))
			.ToListAsync();
	}

    public async Task<List<Skill>> GetSkillsByNamesAsync(List<string> skillNames)
    {
        return await _context.Skills
            .Where(s => skillNames.Contains(s.Name))
            .ToListAsync();
    }
}
