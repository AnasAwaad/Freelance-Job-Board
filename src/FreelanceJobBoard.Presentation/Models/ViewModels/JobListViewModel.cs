using FreelanceJobBoard.Application.Features.Skills.DTOs;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class JobListViewModel
{
	public int Id { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public string ClientName { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime CreatedOn { get; set; }
	public List<string> Tags { get; set; } = new();
	public string ClientProfileImage { get; set; }
	public string Status { get; set; }
	public IEnumerable<PublicSkillViewModel> Skills { get; set; }
	public string TimeAgo
	{
		get
		{
			var timeSpan = DateTime.Now - CreatedOn;

			if (timeSpan.TotalMinutes < 1)
				return "Just now";
			if (timeSpan.TotalMinutes < 60)
				return $"{timeSpan.Minutes} minutes ago";
			if (timeSpan.TotalHours < 24)
				return $"{timeSpan.Hours} hours ago";
			else
				return $"{timeSpan.Days} days ago";
		}
	}
}
