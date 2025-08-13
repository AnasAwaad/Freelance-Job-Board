using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class JobViewModel
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientProfileImageUrl { get; set; }
    public decimal ClientAverageRating { get; set; }
    public int ClientTotalReviews { get; set; }
    public string? AssignedFreelancerName { get; set; }
    public string? AssignedFreelancerProfileImageUrl { get; set; }
    public decimal AssignedFreelancerAverageRating { get; set; }
    public int AssignedFreelancerTotalReviews { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = null!;
    public string? RequiredSkills { get; set; }
    public string? Tags { get; set; }
    public int ViewsCount { get; set; }
    public bool IsApproved { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public ICollection<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    public ICollection<SkillViewModel> Skills { get; set; } = new List<SkillViewModel>();
    
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