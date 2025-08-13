using MediatR;

namespace FreelanceJobBoard.Application.Features.Reviews.Commands.QuickReview;

public class QuickReviewCommand : IRequest<int>
{
    public int JobId { get; set; }
    public string RevieweeId { get; set; } = string.Empty;
    public string ReviewType { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
}