using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;
public interface IJobRepository : IGenericRepository<Job>
{
	Task<Job?> GetJobWithCategoriesAndSkillsAsync(int id);
	Task<Job?> GetJobWithDetailsAsync(int id);
	Task<(int, IEnumerable<Job>)> GetAllMatchingAsync(int pageNumber, int pageSize, string? search, string? sortBy, SortDirection sortDirection);

	IQueryable<Job> GetJobWithDetailsQuery(int id);
	IQueryable<Job> GetAllWithClientQueryable(string? status);
	Task<IEnumerable<Job>> GetJobsByClientIdAsync(int clientId);
}
