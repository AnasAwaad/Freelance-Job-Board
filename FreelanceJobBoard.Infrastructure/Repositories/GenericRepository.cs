using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
	protected readonly ApplicationDbContext _context;
	protected readonly DbSet<T> _dbSet;
	public GenericRepository(ApplicationDbContext context)
	{
		_context = context;
		_dbSet = _context.Set<T>();
	}
	public void Delete(string id)
	{
		var entity = _dbSet.Find(id);
		if (entity != null)
		{
			_dbSet.Remove(entity);
		}
	}

	public async Task<IEnumerable<T>> GetAllAsync()
	{
		return await _dbSet.AsNoTracking().ToListAsync();
	}

	public async Task<T?> GetByIdAsync(int id)
	{
		return await _dbSet.FindAsync(id);
	}

	public void Create(T entity)
	{
		_dbSet.Add(entity);
	}

	public void Update(T entity)
	{
		_dbSet.Update(entity);
	}
}
