using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;
public interface IJobRepository : IGenericRepository<Job>
{
	Task<Job?> GetJobWithCategoriesAndSkillsAsync(int id);
}
