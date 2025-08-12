using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class JobRepository : GenericRepository<Job>, IJobRepository
{
	public JobRepository(ApplicationDbContext context, ILogger<GenericRepository<Job>>? logger = null) : base(context, logger)
	{
	}


	public async Task<(int, IEnumerable<Job>)> GetAllMatchingAsync(int pageNumber, int pageSize, string? search, string? sortBy, string sortDirection, int? category, string? statusFilter = null)
	{
		_logger?.LogDebug("🔍 Getting jobs with pagination | PageNumber={PageNumber}, PageSize={PageSize}, Search={Search}, SortBy={SortBy}, SortDirection={SortDirection}, StatusFilter={StatusFilter}", 
			pageNumber, pageSize, search ?? "None", sortBy ?? "None", sortDirection, statusFilter ?? "None");

		try
		{
			var searchValue = search?.ToLower().Trim();


		var query = _context.Jobs
			.Include(j => j.Categories)
			.Where(j => searchValue == null || (j.Title!.ToLower().Contains(searchValue) ||
													(j.Description!.ToLower().Contains(searchValue))));

			if (!string.IsNullOrEmpty(statusFilter))
			{
				query = query.Where(j => j.Status == statusFilter);
				_logger?.LogDebug("📊 Applied status filter | StatusFilter={StatusFilter}", statusFilter);
			}

		if (category.HasValue)
		{
			query = query
				.Where(jc => jc.Categories.Any(c => c.CategoryId == category.Value));
		}

			var totalCount = await query.CountAsync();
			_logger?.LogDebug("📊 Total jobs found | TotalCount={TotalCount}", totalCount);

			if (sortBy is not null)
			{
				var columnsSelector = new Dictionary<string, Expression<Func<Job, object>>>()
				{
					{nameof(Job.Title),j=>j.Title!},
					{nameof(Job.Description),j=>j.Description!},
					{"deadline", j=>j.Deadline},
					{"budget", j=>j.BudgetMax}
				};

				var selectedColumn = columnsSelector.GetValueOrDefault(sortBy.ToLower()) ?? columnsSelector[nameof(Job.Title)];

				query = (sortDirection == SortDirection.Ascending)
					? query.OrderBy(selectedColumn)
					: query.OrderByDescending(selectedColumn);

				_logger?.LogDebug("🔄 Applied sorting | SortBy={SortBy}, SortDirection={SortDirection}", sortBy, sortDirection);
			}
			else
			{
				query = query.OrderByDescending(j => j.CreatedOn);
				_logger?.LogDebug("🔄 Applied default sorting | SortBy=CreatedOn, SortDirection=Descending");
			}

			var jobs = await query
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.Include(j => j.Categories)
					.ThenInclude(c => c.Category)
				.Include(j => j.Skills)
					.ThenInclude(s => s.Skill)
				.ToListAsync();

			_logger?.LogDebug("✅ Jobs retrieved successfully | Count={Count}, TotalCount={TotalCount}", jobs.Count(), totalCount);
			return (totalCount, jobs);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to get jobs with pagination | PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);
			throw;
		}
	}

	public IQueryable<Job> GetAllWithClientQueryable(string? status)
	{
		_logger?.LogDebug("🔍 Getting jobs queryable with client | Status={Status}", status ?? "All");

		try
		{
			var query = _context.Jobs
				.Include(j => j.Client)
					.ThenInclude(c => c!.User)
				.Where(j => string.IsNullOrEmpty(status) || j.Status == status);

			_logger?.LogDebug("✅ Jobs queryable created successfully | Status={Status}", status ?? "All");
			return query;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to create jobs queryable | Status={Status}", status ?? "All");
			throw;
		}
	}

	public async Task<Job?> GetJobWithCategoriesAndSkillsAsync(int id)
	{
		_logger?.LogDebug("🔍 Getting job with categories and skills | JobId={JobId}", id);

		try
		{
			var job = await _context.Jobs
				.Include(j => j.Skills)
					.ThenInclude(js => js.Skill)
				.Include(j => j.Categories)
					.ThenInclude(jc => jc.Category)
				.FirstOrDefaultAsync(j => j.Id == id);

			if (job != null)
			{
				_logger?.LogDebug("✅ Job with categories and skills found | JobId={JobId}, CategoryCount={CategoryCount}, SkillCount={SkillCount}", 
					id, job.Categories?.Count ?? 0, job.Skills?.Count ?? 0);
			}
			else
			{
				_logger?.LogDebug("❓ Job with categories and skills not found | JobId={JobId}", id);
			}

			return job;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to get job with categories and skills | JobId={JobId}", id);
			throw;
		}
	}

	public async Task<Job?> GetJobWithDetailsAsync(int id)
	{
		_logger?.LogDebug("🔍 Getting job with details | JobId={JobId}", id);

		try
		{
			var job = await _context.Jobs
				.Include(j => j.Client)
					.ThenInclude(c => c!.User)
				.Include(j => j.Proposals)
					.ThenInclude(p => p.Freelancer)
						.ThenInclude(f => f!.User)
				.FirstOrDefaultAsync(j => j.Id == id);

			if (job != null)
			{
				_logger?.LogDebug("✅ Job with details found | JobId={JobId}, ClientId={ClientId}, ProposalCount={ProposalCount}", 
					id, job.ClientId, job.Proposals?.Count ?? 0);
			}
			else
			{
				_logger?.LogDebug("❓ Job with details not found | JobId={JobId}", id);
			}

			return job;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to get job with details | JobId={JobId}", id);
			throw;
		}
	}

	public async Task<IEnumerable<Job>> GetJobsByClientIdAsync(int clientId)
	{
		_logger?.LogDebug("🔍 Getting jobs by client | ClientId={ClientId}", clientId);

		try
		{
			var jobs = await _context.Jobs
				.Where(j => j.ClientId == clientId)
				.Include(j => j.Categories)
					.ThenInclude(c => c.Category)
				.Include(j => j.Skills)
					.ThenInclude(s => s.Skill)
				.Include(j => j.Proposals)
					.ThenInclude(p => p.Freelancer)
						.ThenInclude(f => f!.User)
				.OrderByDescending(j => j.CreatedOn)
				.ToListAsync();

			_logger?.LogDebug("✅ Jobs retrieved by client | ClientId={ClientId}, JobCount={JobCount}", clientId, jobs.Count());
			return jobs;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to get jobs by client | ClientId={ClientId}", clientId);
			throw;
		}
	}

	public async Task<IEnumerable<Job>> GetJobsByFreelancerIdAsync(int freelancerId)
	{
		_logger?.LogDebug("🔍 Getting jobs by freelancer | FreelancerId={FreelancerId}", freelancerId);

		try
		{
			var jobs = await _context.Jobs
				.Where(j => j.Proposals.Any(p => p.FreelancerId == freelancerId && p.Status == ProposalStatus.Accepted))
				.Include(j => j.Categories)
					.ThenInclude(c => c.Category)
				.Include(j => j.Skills)
					.ThenInclude(s => s.Skill)
				.Include(j => j.Proposals)
					.ThenInclude(p => p.Freelancer)
						.ThenInclude(f => f!.User)
				.Include(j => j.Client)
					.ThenInclude(c => c!.User)
				.OrderByDescending(j => j.CreatedOn)
				.ToListAsync();

			_logger?.LogDebug("✅ Jobs retrieved by freelancer | FreelancerId={FreelancerId}, JobCount={JobCount}", freelancerId, jobs.Count());
			return jobs;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to get jobs by freelancer | FreelancerId={FreelancerId}", freelancerId);
			throw;
		}
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
			_logger?.LogDebug("✅ Job details query created successfully | JobId={JobId}", id);
			return query;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to create job details query | JobId={JobId}", id);
			throw;
		}
	}
}
