using FreelanceJobBoard.Application.Interfaces.Repositories;

namespace FreelanceJobBoard.Application.Interfaces;
public interface IUnitOfWork
{
	public ICategoryRepository Categories { get; }
	public ISkillRepository Skills { get; }
	public IJobRepository Jobs { get; }
	Task SaveChangesAsync();

}
