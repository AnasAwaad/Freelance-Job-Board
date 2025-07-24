using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class JobRepository : GenericRepository<Job>, IJobRepository
{
	public JobRepository(ApplicationDbContext context) : base(context)
	{
	}

}
