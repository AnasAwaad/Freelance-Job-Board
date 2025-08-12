using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class JobCategoryRepository : GenericRepository<JobCategory>, IJobCategoryRepository
{
	public JobCategoryRepository(ApplicationDbContext context, ILogger<GenericRepository<JobCategory>>? logger = null) : base(context, logger)
	{
	}

}
