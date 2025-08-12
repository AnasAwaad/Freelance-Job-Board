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


	public async Task<(int, IEnumerable<Job>)> GetAllMatchingAsync(int pageNumber, int pageSize, string? search, string? sortBy, SortDirection sortDirection, string? statusFilter = null)
	{
		var searchValue = search?.ToLower().Trim();

		var query = _context.Jobs
			.Where(j => searchValue == null || (j.Title!.ToLower().Contains(searchValue) ||
														(j.Description!.ToLower().Contains(searchValue))));

		if (!string.IsNullOrEmpty(statusFilter))
		{
			query = query.Where(j => j.Status == statusFilter);
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
				.ThenInclude(p => p.Freelancer)
					.ThenInclude(f => f.User)
			.OrderByDescending(j => j.CreatedOn)
			.ToListAsync();
	}

	public async Task<IEnumerable<Job>> GetJobsByFreelancerIdAsync(int freelancerId)
	{
		return await _context.Jobs
			.Where(j => j.Proposals.Any(p => p.FreelancerId == freelancerId && p.Status == ProposalStatus.Accepted))
			.Include(j => j.Categories)
				.ThenInclude(c => c.Category)
			.Include(j => j.Skills)
				.ThenInclude(s => s.Skill)
			.Include(j => j.Proposals)
				.ThenInclude(p => p.Freelancer)
					.ThenInclude(f => f.User)
			.Include(j => j.Client)
				.ThenInclude(c => c.User)
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
			.Include(j => j.Reviews)
			.Where(j => j.Id == id);

	}
}
