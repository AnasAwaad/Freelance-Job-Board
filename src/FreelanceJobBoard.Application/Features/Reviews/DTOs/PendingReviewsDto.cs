namespace FreelanceJobBoard.Application.Features.Reviews.DTOs;

public class PendingReviewsDto
{
    public string UserId { get; set; } = string.Empty;
    public int TotalPending { get; set; }
    public List<PendingReviewItemDto> PendingReviews { get; set; } = new();
}

public class PendingReviewItemDto
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string ReviewType { get; set; } = string.Empty;
    public string RevieweeName { get; set; } = string.Empty;
    public string RevieweeId { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
    public bool IsUrgent { get; set; }
}

public class QuickReviewDto
{
    public int JobId { get; set; }
    public string RevieweeId { get; set; } = string.Empty;
    public string ReviewType { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
}