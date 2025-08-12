using MediatR;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetPendingReviewsCount;

public class GetPendingReviewsCountQuery : IRequest<int>
{
    public string UserId { get; set; } = string.Empty;
}