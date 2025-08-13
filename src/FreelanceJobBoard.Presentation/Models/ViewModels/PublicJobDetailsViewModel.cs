using FreelanceJobBoard.Application.Features.Skills.DTOs;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class PublicJobDetailsViewModel
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string Status { get; set; }
	public string? Tags { get; set; }
	public PublicClientViewModel Client { get; set; }
	public DateTime? LastUpdatedOn { get; set; }
	public IEnumerable<PublicSkillViewModel> Skills { get; set; }
	public IEnumerable<JobListViewModel> RelatedJobs { get; set; }
}
