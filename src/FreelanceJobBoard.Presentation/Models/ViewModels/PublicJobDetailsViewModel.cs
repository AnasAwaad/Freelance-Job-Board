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
	public DateTime CreatedOn { get; set; }
	public DateTime? LastUpdatedOn { get; set; }
	public IEnumerable<PublicSkillViewModel> Skills { get; set; }
	public IEnumerable<JobListViewModel> RelatedJobs { get; set; }
	
	public string TimeAgo
	{
		get
		{
			var timeSpan = DateTime.Now - CreatedOn;

			if (timeSpan.TotalMinutes < 1)
				return "Just now";
			if (timeSpan.TotalMinutes < 60)
				return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
			if (timeSpan.TotalHours < 24)
				return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours == 1 ? "" : "s")} ago";
			if (timeSpan.TotalDays < 30)
				return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays == 1 ? "" : "s")} ago";
			if (timeSpan.TotalDays < 365)
				return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) == 1 ? "" : "s")} ago";
			else
				return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) == 1 ? "" : "s")} ago";
		}
	}
}
