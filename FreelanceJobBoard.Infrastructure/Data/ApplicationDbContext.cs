using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FreelanceJobBoard.Infrastructure.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{

	public DbSet<Freelancer> Freelancers => Set<Freelancer>();
	public DbSet<Client> Clients => Set<Client>();
	public DbSet<Job> Jobs => Set<Job>();
	public DbSet<JobCategory> JobCategories => Set<JobCategory>();
	public DbSet<Category> Categories => Set<Category>();
	public DbSet<JobView> JobViews => Set<JobView>();
	public DbSet<Review> Reviews => Set<Review>();
	public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractStatus> ContractStatuses => Set<ContractStatus>();
	public DbSet<Attachment> Attachments => Set<Attachment>();
	public DbSet<ProposalAttachment> ProposalAttachments => Set<ProposalAttachment>();
	public DbSet<JobAttachment> JobAttachments => Set<JobAttachment>();
	public DbSet<Skill> Skills => Set<Skill>();
	public DbSet<JobSkill> JobSkills => Set<JobSkill>();
	public DbSet<FreelancerSkill> FreelancerSkills => Set<FreelancerSkill>();
	public DbSet<Certification> Certifications => Set<Certification>();
	public DbSet<Notification> Notifications => Set<Notification>();
	public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();


	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}
}