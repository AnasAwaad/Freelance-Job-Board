using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;
public interface IJobRepository : IGenericRepository<Job>
{
	Task<Job?> GetJobWithCategoriesAndSkillsAsync(int id);
	Task<Job?> GetJobWithDetailsAsync(int id);
	Task<(int, IEnumerable<Job>)> GetAllMatchingAsync(int pageNumber, int pageSize, string? search, string? sortBy, string sortDirection, int? category, string? statusFilter = null);

	IQueryable<Job> GetJobWithDetailsQuery(int id);
	IQueryable<Job> GetAllWithClientQueryable(string? status);
	Task<IEnumerable<Job>> GetJobsByClientIdAsync(int clientId);
	IQueryable<Job> GetRecentJobsQueryable(int numOfJobs);
	IQueryable<Job> GetRelatedJobsQueryable(int jobId);
	IQueryable<Job> getPublicJobDetails(int jobId);
	Task<IEnumerable<JobSearchDto>> SearchJobsAsync(string query, int limit);
	Task<IEnumerable<Job>> GetJobsByFreelancerIdAsync(int freelancerId);
	Task<int> GetNumberOfJobs();
}
