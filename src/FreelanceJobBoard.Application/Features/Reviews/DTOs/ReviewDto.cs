namespace FreelanceJobBoard.Application.Features.Reviews.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public string RevieweeId { get; set; } = string.Empty;
    public string RevieweeName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string ReviewType { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public int JobId { get; set; }
    public string RevieweeId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string ReviewType { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
}

public class ReviewSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewDto> RecentReviews { get; set; } = new();
}