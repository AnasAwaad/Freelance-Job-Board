using AutoMapper;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviews;

public class GetReviewsQueryHandler : IRequestHandler<GetReviewsQuery, IEnumerable<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReviewsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReviewDto>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Review> reviews;

        if (request.JobId.HasValue)
        {
            var review = await _unitOfWork.Reviews.GetByJobIdAsync(request.JobId.Value);
            reviews = review != null ? new[] { review } : Enumerable.Empty<Review>();
        }
        else if (!string.IsNullOrEmpty(request.UserId))
        {
            reviews = request.OnlyVisible 
                ? await _unitOfWork.Reviews.GetVisibleReviewsByRevieweeIdAsync(request.UserId)
                : await _unitOfWork.Reviews.GetByRevieweeIdAsync(request.UserId);
        }
        else
        {
            reviews = await _unitOfWork.Reviews.GetAllAsync();
            if (request.OnlyVisible)
            {
                reviews = reviews.Where(r => r.IsVisible && r.IsActive);
            }
        }

        return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
    }
}