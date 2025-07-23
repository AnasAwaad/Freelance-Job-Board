using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Infrastructure.Data;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class UnitOfWork : IUnitOfWork
{
	public ICategoryRepository Categories { get; }


	public UnitOfWork(ApplicationDbContext context)
	{
		Categories = new CategoryRepository(context);
	}

	public void Dispose()
	{
		System.GC.SuppressFinalize(this);
	}
}