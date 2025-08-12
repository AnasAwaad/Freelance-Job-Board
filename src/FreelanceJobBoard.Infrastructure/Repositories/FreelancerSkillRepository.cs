using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

internal class FreelancerSkillRepository : GenericRepository<FreelancerSkill>, IFreelancerSkillRepository
{
    public FreelancerSkillRepository(ApplicationDbContext context, ILogger<GenericRepository<FreelancerSkill>>? logger = null) : base(context, logger)
    {
    }

    public async Task<IEnumerable<FreelancerSkill>> GetBySkillIdAsync(int skillId)
    {
        return await _context.FreelancerSkills
            .Where(fs => fs.SkillId == skillId)
            .ToListAsync();
    }
}