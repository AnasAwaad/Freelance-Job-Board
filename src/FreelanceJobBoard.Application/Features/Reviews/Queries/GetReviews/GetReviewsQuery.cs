using MediatR;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviews;

public class GetReviewsQuery : IRequest<IEnumerable<ReviewDto>>
{
    public string? UserId { get; set; }
    public int? JobId { get; set; }
    public bool OnlyVisible { get; set; } = true;
}