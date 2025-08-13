using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class JobSkillRepository : GenericRepository<JobSkill>, IJobSkillRepository
{
	public JobSkillRepository(ApplicationDbContext context, ILogger<GenericRepository<JobSkill>>? logger = null) : base(context, logger)
	{
	}

}
