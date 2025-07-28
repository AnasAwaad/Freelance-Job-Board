using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;

internal class FreelancerSkillRepository : GenericRepository<FreelancerSkill>, IFreelancerSkillRepository
{
    public FreelancerSkillRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FreelancerSkill>> GetBySkillIdAsync(int skillId)
    {
        return await _context.FreelancerSkills
            .Where(fs => fs.SkillId == skillId)
            .ToListAsync();
    }
}