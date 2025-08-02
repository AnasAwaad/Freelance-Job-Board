using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IFreelancerSkillRepository : IGenericRepository<FreelancerSkill>
{
    Task<IEnumerable<FreelancerSkill>> GetBySkillIdAsync(int skillId);
}