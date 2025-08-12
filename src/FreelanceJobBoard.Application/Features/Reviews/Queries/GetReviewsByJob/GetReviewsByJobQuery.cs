using MediatR;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewsByJob;

public class GetReviewsByJobQuery : IRequest<IEnumerable<ReviewDto>>
{
    public int JobId { get; set; }
}