using MediatR;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using AutoMapper;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewsByUser;

public class GetReviewsByUserQueryHandler : IRequestHandler<GetReviewsByUserQuery, ReviewSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReviewsByUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReviewSummaryDto> Handle(GetReviewsByUserQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Reviews.GetVisibleReviewsByRevieweeIdAsync(request.UserId);
        var averageRating = await _unitOfWork.Reviews.GetAverageRatingByRevieweeIdAsync(request.UserId);
        var totalReviews = await _unitOfWork.Reviews.GetTotalReviewCountByRevieweeIdAsync(request.UserId);

        // Get user name - we'll need to enhance this to get the actual user name
        var userName = await GetUserNameAsync(request.UserId);

        var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);

        return new ReviewSummaryDto
        {
            UserId = request.UserId,
            UserName = userName,
            AverageRating = averageRating,
            TotalReviews = totalReviews,
            RecentReviews = reviewDtos
        };
    }

    private async Task<string> GetUserNameAsync(string userId)
    {
        // First try to get from client
        var client = await _unitOfWork.Clients.GetByUserIdAsync(userId);
        if (client?.User != null)
            return client.User.FullName ?? "Unknown User";

        // Then try to get from freelancer
        var freelancer = await _unitOfWork.Freelancers.GetByUserIdAsync(userId);
        if (freelancer?.User != null)
            return freelancer.User.FullName ?? "Unknown User";

        return "Unknown User";
    }
}