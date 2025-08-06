namespace FreelanceJobBoard.Application.Features.Reviews.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string ReviewerId { get; set; } = null!;
    public string ReviewerName { get; set; } = null!;
    public string RevieweeId { get; set; } = null!;
    public string RevieweeName { get; set; } = null!;
    public int Rating { get; set; }
    public string Comment { get; set; } = null!;
    public string ReviewType { get; set; } = null!;
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public int JobId { get; set; }
    public string RevieweeId { get; set; } = null!;
    public int Rating { get; set; }
    public string Comment { get; set; } = null!;
    public string ReviewType { get; set; } = null!;
    public bool IsVisible { get; set; } = true;
}

public class ReviewSummaryDto
{
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewDto> RecentReviews { get; set; } = new();
}