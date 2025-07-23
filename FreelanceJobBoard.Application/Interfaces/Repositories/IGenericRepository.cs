namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
	Task CreateAsync(T entity);
	void Delete(string id);
	Task<IEnumerable<T>> GetAllAsync();
	Task<T?> GetByIdAsync(int id);
	void Update(T entity);
}