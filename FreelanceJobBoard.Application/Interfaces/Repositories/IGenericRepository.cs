
namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
	void Create(T entity);
	void Delete(string id);
	Task<IEnumerable<T>> GetAllAsync();
	T? GetById(int id);
	void Update(T entity);
}