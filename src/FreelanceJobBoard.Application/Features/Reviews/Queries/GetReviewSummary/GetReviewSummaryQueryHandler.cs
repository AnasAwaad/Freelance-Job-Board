using AutoMapper;
using FreelanceJobBoard.Domain.Exceptions;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewSummary;

public class GetReviewSummaryQueryHandler : IRequestHandler<GetReviewSummaryQuery, ReviewSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetReviewSummaryQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<ReviewSummaryDto> Handle(GetReviewSummaryQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException(nameof(ApplicationUser), request.UserId);

        var averageRating = await _unitOfWork.Reviews.GetAverageRatingByRevieweeIdAsync(request.UserId);
        var totalReviews = await _unitOfWork.Reviews.GetTotalReviewCountByRevieweeIdAsync(request.UserId);

        var recentReviews = await _unitOfWork.Reviews.GetVisibleReviewsByRevieweeIdAsync(request.UserId);
        var recentReviewsList = recentReviews.Take(request.MaxRecentReviews).ToList();

        var reviewDtos = _mapper.Map<List<ReviewDto>>(recentReviewsList);

        return new ReviewSummaryDto
        {
            UserId = request.UserId,
            UserName = user.FullName,
            AverageRating = averageRating,
            TotalReviews = totalReviews,
            RecentReviews = reviewDtos
        };
    }
}