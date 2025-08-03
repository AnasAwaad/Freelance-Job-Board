using MediatR;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewSummary;

public class GetReviewSummaryQuery : IRequest<ReviewSummaryDto>
{
    public string UserId { get; set; } = null!;
    public int MaxRecentReviews { get; set; } = 5;
}