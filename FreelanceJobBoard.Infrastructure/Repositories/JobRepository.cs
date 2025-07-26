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


	public async Task<(int, IEnumerable<Job>)> GetAllMatchingAsync(int pageNumber, int pageSize, string? search, string? sortBy, SortDirection sortDirection)
	{
		var searchValue = search?.ToLower().Trim();

		var query = _context.Jobs
			.Where(j => searchValue == null || (j.Title!.ToLower().Contains(searchValue) ||
														(j.Description!.ToLower().Contains(searchValue))));

		var totalCount = await query.CountAsync();


		if (sortBy is not null)
		{
			var columnsSelector = new Dictionary<string, Expression<Func<Job, object>>>()
			{
				{nameof(Job.Title),j=>j.Title},
				{nameof(Job.Description),j=>j.Description}
			};

			var selectedColumn = columnsSelector[sortBy];

			query = (sortDirection == SortDirection.Ascending)
				? query.OrderBy(selectedColumn)
				: query.OrderByDescending(selectedColumn);
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

	public async Task<Job?> GetJobWithCategoriesAndSkillsAsync(int id)
	{
		return await _context.Jobs
			.Include(j => j.Skills)
			.Include(j => j.Categories)
			.FirstOrDefaultAsync(j => j.Id == id);
	}
}
