using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;
public interface ISkillRepository : IGenericRepository<Skill>
{
	Task<List<Skill>> GetSkillsByIdsAsync(IEnumerable<int> skillIds);
}
