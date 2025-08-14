using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.Extensions.Logging;

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
	public IReviewRepository Reviews { get; }
	public IContractRepository Contracts { get; }
	public IContractVersionRepository ContractVersions { get; }
	public IContractChangeRequestRepository ContractChangeRequests { get; }
	public IAttachmentRepository Attachments { get; }

	public UnitOfWork(ApplicationDbContext context, ILoggerFactory? loggerFactory = null)
	{
		_context = context;
		Categories = new CategoryRepository(context, loggerFactory?.CreateLogger<GenericRepository<Category>>());
		Skills = new SkillRepository(context, loggerFactory?.CreateLogger<GenericRepository<Skill>>());
		Jobs = new JobRepository(context, loggerFactory?.CreateLogger<GenericRepository<Job>>());
		JobCategories = new JobCategoryRepository(context, loggerFactory?.CreateLogger<GenericRepository<JobCategory>>());
		JobSkills = new JobSkillRepository(context, loggerFactory?.CreateLogger<GenericRepository<JobSkill>>());
		Proposals = new ProposalRepository(context, loggerFactory?.CreateLogger<GenericRepository<Proposal>>());
		Clients = new ClientRepository(context, loggerFactory?.CreateLogger<GenericRepository<Client>>());
		Freelancers = new FreelancerRepository(context, loggerFactory?.CreateLogger<GenericRepository<Freelancer>>());
		FreelancerSkills = new FreelancerSkillRepository(context, loggerFactory?.CreateLogger<GenericRepository<FreelancerSkill>>());
		Notifications = new NotificationRepository(context, loggerFactory?.CreateLogger<GenericRepository<Notification>>());
		Reviews = new ReviewRepository(context, loggerFactory?.CreateLogger<GenericRepository<Review>>());
		Contracts = new ContractRepository(context, loggerFactory?.CreateLogger<GenericRepository<Contract>>());
		ContractVersions = new ContractVersionRepository(context, loggerFactory?.CreateLogger<GenericRepository<ContractVersion>>());
		ContractChangeRequests = new ContractChangeRequestRepository(context, loggerFactory?.CreateLogger<GenericRepository<ContractChangeRequest>>());
		Attachments = new AttachmentRepository(context, loggerFactory?.CreateLogger<GenericRepository<Attachment>>());
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