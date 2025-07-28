using FreelanceJobBoard.Domain.Common;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Domain.Entities;
public class Job : BaseEntity
{
	public int Id { get; set; }
	public int? ClientId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string Status { get; set; } = JobStatus.Open;
	public string? RequiredSkills { get; set; }
	public string? Tags { get; set; }
	public int ViewsCount { get; set; }
	public bool IsApproved { get; set; }
	public int? ApprovedBy { get; set; }
	public Client? Client { get; set; }
	public ICollection<JobCategory> Categories { get; set; } = new List<JobCategory>();
	public ICollection<JobAttachment> Attachments { get; set; } = new List<JobAttachment>();
	public ICollection<JobView> Views { get; set; } = new List<JobView>();
	public ICollection<JobSkill> Skills { get; set; } = new List<JobSkill>();
	public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
	public Review Review { get; set; }
}
