using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
	protected readonly ApplicationDbContext _context;
	protected readonly DbSet<T> _dbSet;
	protected readonly ILogger<GenericRepository<T>>? _logger;

	public GenericRepository(ApplicationDbContext context, ILogger<GenericRepository<T>>? logger = null)
	{
		_context = context;
		_dbSet = _context.Set<T>();
		_logger = logger;
	}

	public void Delete(T entity)
	{
		_logger?.LogDebug("🗑️ Deleting entity | EntityType={EntityType}", typeof(T).Name);
		_dbSet.Remove(entity);
	}

	public async Task<IEnumerable<T>> GetAllAsync()
	{
		_logger?.LogDebug("📋 Getting all entities | EntityType={EntityType}", typeof(T).Name);
		try
		{
			var entities = await _dbSet.AsNoTracking().ToListAsync();
			_logger?.LogDebug("✅ Retrieved entities | EntityType={EntityType}, Count={Count}", typeof(T).Name, entities.Count());
			return entities;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to retrieve all entities | EntityType={EntityType}", typeof(T).Name);
			throw;
		}
	}

	public async Task<T?> GetByIdAsync(int id)
	{
		_logger?.LogDebug("🔍 Getting entity by ID | EntityType={EntityType}, Id={Id}", typeof(T).Name, id);
		try
		{
			var entity = await _dbSet.FindAsync(id);
			if (entity != null)
			{
				_logger?.LogDebug("✅ Entity found | EntityType={EntityType}, Id={Id}", typeof(T).Name, id);
			}
			else
			{
				_logger?.LogDebug("❓ Entity not found | EntityType={EntityType}, Id={Id}", typeof(T).Name, id);
			}
			return entity;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to retrieve entity by ID | EntityType={EntityType}, Id={Id}", typeof(T).Name, id);
			throw;
		}
	}

	public async Task CreateAsync(T entity)
	{
		_logger?.LogDebug("🆕 Creating new entity | EntityType={EntityType}", typeof(T).Name);
		try
		{
			await _dbSet.AddAsync(entity);
			_logger?.LogDebug("✅ Entity added to context | EntityType={EntityType}", typeof(T).Name);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to create entity | EntityType={EntityType}", typeof(T).Name);
			throw;
		}
	}

	public void Update(T entity)
	{
		_logger?.LogDebug("📝 Updating entity | EntityType={EntityType}", typeof(T).Name);
		try
		{
			_dbSet.Update(entity);
			_logger?.LogDebug("✅ Entity updated in context | EntityType={EntityType}", typeof(T).Name);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to update entity | EntityType={EntityType}", typeof(T).Name);
			throw;
		}
	}

	public void RemoveRange(IEnumerable<T> entities)
	{
		var count = entities.Count();
		_logger?.LogDebug("🗑️ Removing multiple entities | EntityType={EntityType}, Count={Count}", typeof(T).Name, count);
		try
		{
			_dbSet.RemoveRange(entities);
			_logger?.LogDebug("✅ Entities removed from context | EntityType={EntityType}, Count={Count}", typeof(T).Name, count);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to remove entities | EntityType={EntityType}, Count={Count}", typeof(T).Name, count);
			throw;
		}
	}
}
