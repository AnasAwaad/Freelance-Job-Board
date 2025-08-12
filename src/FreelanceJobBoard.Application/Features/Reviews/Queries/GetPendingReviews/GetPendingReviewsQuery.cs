using MediatR;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetPendingReviews;

public class GetPendingReviewsQuery : IRequest<PendingReviewsDto>
{
    public string UserId { get; set; } = string.Empty;
}