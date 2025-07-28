using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class JobCategoryRepository : GenericRepository<JobCategory>, IJobCategoryRepository
{
	public JobCategoryRepository(ApplicationDbContext context) : base(context)
	{
	}

}
