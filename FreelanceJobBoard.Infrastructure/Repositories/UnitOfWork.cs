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
	public IJobCategoryRepository JobCategories { get; }
	public IJobSkillRepository JobSkills { get; }
	public IProposalRepository Proposals { get; }
	public IClientRepository Clients { get; }
	public IFreelancerRepository Freelancers { get; }
	public IFreelancerSkillRepository FreelancerSkills { get; }
	public INotificationRepository Notifications { get; }

	public UnitOfWork(ApplicationDbContext context)
	{
		_context = context;
		Categories = new CategoryRepository(context);
		Skills = new SkillRepository(context);
		Jobs = new JobRepository(context);
		JobCategories = new JobCategoryRepository(context);
		JobSkills = new JobSkillRepository(context);
		Proposals = new ProposalRepository(context);
		Clients = new ClientRepository(context);
		Freelancers = new FreelancerRepository(context);
		FreelancerSkills = new FreelancerSkillRepository(context);
		Notifications = new NotificationRepository(context);
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