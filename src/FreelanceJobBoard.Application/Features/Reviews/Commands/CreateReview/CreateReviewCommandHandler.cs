using AutoMapper;
using FreelanceJobBoard.Domain.Exceptions;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateReviewCommandHandler> _logger;

    public CreateReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateReviewCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<int> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedAccessException("User must be authenticated to create a review.");

        var canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(request.JobId, currentUserId);
        if (!canReview)
            throw new InvalidOperationException("You cannot review this job. The job must be completed and you must be either the client or the accepted freelancer.");

        var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedJobAsync(request.JobId, currentUserId);
        if (hasReviewed)
            throw new InvalidOperationException("You have already reviewed this job.");

        var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(request.JobId);
        if (job == null)
            throw new NotFoundException(nameof(Job), request.JobId.ToString());

        await ValidateRevieweeAsync(request, currentUserId, job);

        var review = new Review
        {
            JobId = request.JobId,
            ReviewerId = currentUserId,
            RevieweeId = request.RevieweeId,
            Rating = request.Rating,
            Comment = request.Comment,
            ReviewType = request.ReviewType,
            IsVisible = request.IsVisible,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        await _unitOfWork.Reviews.CreateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        await UpdateAverageRating(request.RevieweeId);

        _logger.LogInformation("Review created successfully. ReviewId: {ReviewId}, JobId: {JobId}, ReviewerId: {ReviewerId}",
            review.Id, request.JobId, currentUserId);

        return review.Id;
    }

    private async Task ValidateRevieweeAsync(CreateReviewCommand request, string currentUserId, Job job)
    {
        var isClient = job.Client?.UserId == currentUserId;
        
        var acceptedProposal = job.Proposals?.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
        var isFreelancer = acceptedProposal?.Freelancer?.UserId == currentUserId;

        if (request.ReviewType == ReviewType.ClientToFreelancer)
        {
            if (!isClient)
                throw new UnauthorizedAccessException("Only the client can create a ClientToFreelancer review.");
            
            if (acceptedProposal?.Freelancer?.UserId != request.RevieweeId)
                throw new ArgumentException("Reviewee must be the accepted freelancer for this job.");
        }
        else if (request.ReviewType == ReviewType.FreelancerToClient)
        {
            if (!isFreelancer)
                throw new UnauthorizedAccessException("Only the accepted freelancer can create a FreelancerToClient review.");
            
            if (job.Client?.UserId != request.RevieweeId)
                throw new ArgumentException("Reviewee must be the client for this job.");
        }
        else
        {
            throw new ArgumentException("Invalid review type.");
        }
    }

    private async Task UpdateAverageRating(string userId)
    {
        var averageRating = await _unitOfWork.Reviews.GetAverageRatingByRevieweeIdAsync(userId);
        var totalReviews = await _unitOfWork.Reviews.GetTotalReviewCountByRevieweeIdAsync(userId);

        var client = await _unitOfWork.Clients.GetByUserIdAsync(userId);
        if (client != null)
        {
            client.AverageRating = averageRating;
            client.TotalReviews = totalReviews;
            _unitOfWork.Clients.Update(client);
        }
        else
        {
            var freelancer = await _unitOfWork.Freelancers.GetByUserIdAsync(userId);
            if (freelancer != null)
            {
                freelancer.AverageRating = averageRating;
                freelancer.TotalReviews = totalReviews;
                _unitOfWork.Freelancers.Update(freelancer);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}