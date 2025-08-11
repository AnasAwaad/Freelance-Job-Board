using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class JobRepository : GenericRepository<Job>, IJobRepository
{
	public JobRepository(ApplicationDbContext context) : base(context)
	{
	}


	public async Task<(int, IEnumerable<Job>)> GetAllMatchingAsync(int pageNumber, int pageSize, string? search, string? sortBy, string sortDirection, int? category, string? statusFilter = null)
	{
		var searchValue = search?.ToLower().Trim();


		var query = _context.Jobs
			.Include(j => j.Categories)
			.Where(j => searchValue == null || (j.Title!.ToLower().Contains(searchValue) ||
													(j.Description!.ToLower().Contains(searchValue))));

		// Apply status filter if provided
		if (!string.IsNullOrEmpty(statusFilter))
		{
			query = query.Where(j => j.Status == statusFilter);
		}

		if (category.HasValue)
		{
			query = query
				.Where(jc => jc.Categories.Any(c => c.CategoryId == category.Value));
		}


		var totalCount = await query.CountAsync();

		if (sortBy is not null)
		{
			var columnsSelector = new Dictionary<string, Expression<Func<Job, object>>>()
			{
				{nameof(Job.Title),j=>j.Title},
				{nameof(Job.Description),j=>j.Description},
				{"deadline", j=>j.Deadline},
				{"budget", j=>j.BudgetMax}
			};

			var selectedColumn = columnsSelector.GetValueOrDefault(sortBy.ToLower()) ?? columnsSelector[nameof(Job.Title)];

			query = (sortDirection == SortDirection.Ascending)
				? query.OrderBy(selectedColumn)
				: query.OrderByDescending(selectedColumn);
		}
		else
		{
			// Default ordering by creation date (newest first)
			query = query.OrderByDescending(j => j.CreatedOn);
		}

		var jobs = await query
			.Skip(pageSize * (pageNumber - 1))
			.Take(pageSize)
			.Include(j => j.Categories)
				.ThenInclude(c => c.Category)
			.Include(j => j.Skills)
				.ThenInclude(s => s.Skill)
			.ToListAsync();

		return (totalCount, jobs);
	}

	public IQueryable<Job> GetAllWithClientQueryable(string? status)
	{
		return _context.Jobs
			.Include(j => j.Client)
				.ThenInclude(c => c.User)
			.Where(j => string.IsNullOrEmpty(status) || j.Status == status);

	}

	public async Task<Job?> GetJobWithCategoriesAndSkillsAsync(int id)
	{
		return await _context.Jobs
			.Include(j => j.Skills)
				.ThenInclude(js => js.Skill)
			.Include(j => j.Categories)
				.ThenInclude(jc => jc.Category)
			.FirstOrDefaultAsync(j => j.Id == id);
	}

	public async Task<Job?> GetJobWithDetailsAsync(int id)
	{
		return await _context.Jobs
			.Include(j => j.Client)
				.ThenInclude(c => c.User)
			.Include(j => j.Proposals)
				.ThenInclude(p => p.Freelancer)
					.ThenInclude(f => f.User)
			.FirstOrDefaultAsync(j => j.Id == id);
	}

	public async Task<IEnumerable<Job>> GetJobsByClientIdAsync(int clientId)
	{
		return await _context.Jobs
			.Where(j => j.ClientId == clientId)
			.Include(j => j.Categories)
				.ThenInclude(c => c.Category)
			.Include(j => j.Skills)
				.ThenInclude(s => s.Skill)
			.Include(j => j.Proposals)
			.OrderByDescending(j => j.CreatedOn)
			.ToListAsync();
	}

	public IQueryable<Job> GetJobWithDetailsQuery(int id)
	{
		return _context.Jobs
			.Include(j => j.Proposals)
				.ThenInclude(p => p.Attachments)
					.ThenInclude(a => a.Attachment)
			.Include(j => j.Client)
				.ThenInclude(c => c.User)
			.Include(j => j.Review)
			.Where(j => j.Id == id && j.Status == JobStatus.Open);

	}

	public IQueryable<Job> GetRecentJobsQueryable(int numOfJobs)
	{
		return _context.Jobs.
			Include(j => j.Client)
			.ThenInclude(c => c.User)
			.OrderByDescending(j => j.CreatedOn)
			.Where(j => j.IsActive && j.Status == JobStatus.Open)
			.Take(numOfJobs);
	}

	public IQueryable<Job> getPublicJobDetails(int jobId)
	{
		return _context.Jobs
			.Include(j => j.Client)
				.ThenInclude(c => c.User)
			.Include(j => j.Skills)
				.ThenInclude(js => js.Skill)
			.Include(j => j.Categories)
				.ThenInclude(jc => jc.Category)
			.Where(j => j.IsActive && j.Id == jobId && j.Status == JobStatus.Open);


	}

	public IQueryable<Job> GetRelatedJobsQueryable(int jobId)
	{
		var job = _context.Jobs
			.Include(j => j.Categories)
				.ThenInclude(c => c.Category)
			.FirstOrDefault(j => j.Id == jobId);

		if (job == null)
			return Enumerable.Empty<Job>().AsQueryable();

		var jobCategories = job.Categories.Select(c => c.Category.Id).ToList();

		return _context.Jobs.
			Include(j => j.Client)
			.ThenInclude(c => c.User)
			.OrderByDescending(j => j.CreatedOn)
			.Where(j => j.IsActive && j.Id != jobId && j.Categories.Any(c => jobCategories.Contains(c.CategoryId)));
	}

	public async Task<IEnumerable<JobSearchDto>> SearchJobsAsync(string query, int limit)
	{
		return await _context.Jobs
			.Include(j => j.Client)
				.ThenInclude(c => c.User)
			.Where(j => j.IsActive && j.Status == JobStatus.Open && (j.Title.Contains(query) || j.Description.Contains(query)))
			.Select(j => new JobSearchDto
			{
				ClientName = j.Client.User.FullName,
				BudgetMax = j.BudgetMax.ToString("C"),
				BudgetMin = j.BudgetMin.ToString("C"),
				Id = j.Id,
				Title = j.Title,
				Description = j.Description,
				Deadline = j.Deadline.ToString("yyyy-MM-dd")
			})
			.Take(limit)
			.ToListAsync();
	}
}
