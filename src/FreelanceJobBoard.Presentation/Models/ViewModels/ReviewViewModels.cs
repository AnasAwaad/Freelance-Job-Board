using System.ComponentModel.DataAnnotations;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class CreateReviewViewModel
{
    public int JobId { get; set; }
    
    [Display(Name = "Job Title")]
    public string JobTitle { get; set; } = string.Empty;
    
    [Required]
    public string RevieweeId { get; set; } = string.Empty;
    
    [Display(Name = "Reviewee")]
    public string RevieweeName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
    [Display(Name = "Overall Rating")]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(1000, ErrorMessage = "Comment must not exceed 1000 characters.")]
    [Display(Name = "Review Comment")]
    public string Comment { get; set; } = string.Empty;
    
    [Required]
    public string ReviewType { get; set; } = string.Empty;
    
    [Display(Name = "Make this review visible to other users")]
    public bool IsVisible { get; set; } = true;
    
    // Additional rating dimensions
    [Range(1, 5, ErrorMessage = "Communication rating must be between 1 and 5 stars.")]
    [Display(Name = "Communication")]
    public int? CommunicationRating { get; set; }
    
    [Range(1, 5, ErrorMessage = "Quality rating must be between 1 and 5 stars.")]
    [Display(Name = "Quality of Work")]
    public int? QualityRating { get; set; }
    
    [Range(1, 5, ErrorMessage = "Timeliness rating must be between 1 and 5 stars.")]
    [Display(Name = "Timeliness")]
    public int? TimelinessRating { get; set; }
    
    [Display(Name = "Would you recommend this person?")]
    public bool WouldRecommend { get; set; }
    
    [Display(Name = "Tags (comma-separated)")]
    [StringLength(200, ErrorMessage = "Tags must not exceed 200 characters.")]
    public string? Tags { get; set; }
}

public class JobReviewsViewModel
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public List<ReviewDto> Reviews { get; set; } = new();
    public bool CanCurrentUserReview { get; set; }
    public string? PendingReviewType { get; set; }
}

public class UserReviewsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
    public bool IsCurrentUser { get; set; }
}

public class ReviewSummaryCardViewModel
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
}

// NEW: Enhanced view models for better UX
public class PendingReviewsViewModel
{
    public int TotalPending { get; set; }
    public List<PendingReviewItemViewModel> PendingReviews { get; set; } = new();
}

public class PendingReviewItemViewModel
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string ReviewType { get; set; } = string.Empty;
    public string RevieweeName { get; set; } = string.Empty;
    public string RevieweeId { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
    public bool IsUrgent { get; set; }
    public string CompletedDateFormatted => CompletedDate?.ToString("MMM dd, yyyy") ?? "";
    public string DaysAgo
    {
        get
        {
            if (!CompletedDate.HasValue) return "";
            var days = (DateTime.UtcNow - CompletedDate.Value).TotalDays;
            return days < 1 ? "Today" : $"{(int)days} days ago";
        }
    }
}

public class QuickReviewViewModel
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string RevieweeId { get; set; } = string.Empty;
    public string RevieweeName { get; set; } = string.Empty;
    public string ReviewType { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 5, ErrorMessage = "Please select a rating from 1 to 5 stars.")]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 500 characters.")]
    public string Comment { get; set; } = string.Empty;
    
    public bool IsVisible { get; set; } = true;
}