using MediatR;

namespace FreelanceJobBoard.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommand : IRequest<int>
{
    public int JobId { get; set; }
    public string RevieweeId { get; set; } = null!;
    public int Rating { get; set; }
    public string Comment { get; set; } = null!;
    public string ReviewType { get; set; } = null!;
    public bool IsVisible { get; set; } = true;
    public int? CommunicationRating { get; set; }
    public int? QualityRating { get; set; }
    public int? TimelinessRating { get; set; }
    public bool WouldRecommend { get; set; }
    public string? Tags { get; set; }
}