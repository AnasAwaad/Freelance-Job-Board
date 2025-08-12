using MediatR;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewsByUser;

public class GetReviewsByUserQuery : IRequest<ReviewSummaryDto>
{
    public string UserId { get; set; } = string.Empty;
}