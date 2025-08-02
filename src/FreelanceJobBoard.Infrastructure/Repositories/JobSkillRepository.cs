using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class JobSkillRepository : GenericRepository<JobSkill>, IJobSkillRepository
{
	public JobSkillRepository(ApplicationDbContext context) : base(context)
	{
	}
}
