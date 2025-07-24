using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Infrastructure.Data;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class UnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _context;
	public ICategoryRepository Categories { get; }
	public ISkillRepository Skills { get; }
	public IJobRepository Jobs { get; }


	public UnitOfWork(ApplicationDbContext context)
	{
		_context = context;
		Categories = new CategoryRepository(context);
		Skills = new SkillRepository(context);
		Jobs = new JobRepository(context);
	}

	public void Dispose()
	{
		_context.Dispose();
	}

	public async Task SaveChangesAsync()
	{
		await _context.SaveChangesAsync();
	}
}