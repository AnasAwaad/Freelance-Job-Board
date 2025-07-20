using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Job : BaseEntity
{
	public int Id { get; set; }
	public int ClientId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string Status { get; set; }
	public string? RequiredSkills { get; set; }
	public string? Tags { get; set; }
	public int ViewsCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public bool IsApproved { get; set; }
	public int? ApprovedBy { get; set; }

	public Client Client { get; set; }
	public ICollection<JobCategory> Categories { get; set; }
	public ICollection<JobAttachment> Attachments { get; set; }
	public ICollection<JobView> Views { get; set; }
	public ICollection<JobSkill> Skills { get; set; }
	public ICollection<Proposal> Proposals { get; set; }
	public Review Review { get; set; }
}
