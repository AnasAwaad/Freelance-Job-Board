using FreelanceJobBoard.Application.Interfaces.Repositories;

namespace FreelanceJobBoard.Application.Interfaces;
public interface IUnitOfWork
{
	public ICategoryRepository Categories { get; }
	public ISkillRepository Skills { get; }
	public IJobRepository Jobs { get; }
	public IJobCategoryRepository JobCategories { get; }
	public IJobSkillRepository JobSkills { get; }
	Task SaveChangesAsync();

}
