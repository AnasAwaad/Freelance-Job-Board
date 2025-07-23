using FreelanceJobBoard.Application.Interfaces.Repositories;

namespace FreelanceJobBoard.Application.Interfaces;
public interface IUnitOfWork
{
	public ICategoryRepository Categories { get; }
}
