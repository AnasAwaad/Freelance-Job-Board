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
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public CreateReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CreateReviewCommandHandler> logger,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _emailService = emailService;
        _notificationService = notificationService;
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
            CommunicationRating = request.CommunicationRating,
            QualityRating = request.QualityRating,
            TimelinessRating = request.TimelinessRating,
            WouldRecommend = request.WouldRecommend,
            Tags = request.Tags,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        await _unitOfWork.Reviews.CreateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        await UpdateAverageRating(request.RevieweeId);

        //await SendReviewNotification(review, job);

        var reviewerName = await GetReviewerName(currentUserId);
        //await _notificationService.NotifyReviewReceivedAsync(
        //    review.Id, 
        //    request.RevieweeId, 
        //    reviewerName, 
        //    job.Title ?? "Unknown Job", 
        //    request.Rating
        //);

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

    //private async Task SendReviewNotification(Review review, Job job)
    //{
    //    try
    //    {
    //        // Get reviewer information
    //        var reviewer = await GetUserDetails(review.ReviewerId);
    //        var reviewee = await GetUserDetails(review.RevieweeId);

    //        if (reviewee?.Email == null)
    //        {
    //            _logger.LogWarning("Cannot send review notification - reviewee email not found for user {RevieweeId}", review.RevieweeId);
    //            return;
    //        }

    //        // Generate star rating display for email
    //        var starRating = new string('?', review.Rating) + new string('?', 5 - review.Rating);

    //        var emailData = new
    //        {
    //            RevieweeName = reviewee.FullName ?? "User",
    //            JobTitle = job.Title ?? "Unknown Job",
    //            StarRating = starRating,
    //            Rating = review.Rating,
    //            ReviewerName = reviewer?.FullName ?? "Anonymous",
    //            Comment = review.Comment ?? "No comment provided"
    //        };

    //        await _emailService.SendTemplateEmailAsync(
    //            reviewee.Email,
    //            "ReviewNotification",
    //            emailData);

    //        _logger.LogInformation("Review notification email sent to {RevieweeEmail} for review {ReviewId}", 
    //            reviewee.Email, review.Id);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to send review notification email for review {ReviewId}", review.Id);
    //        // Don't throw here to avoid breaking the review creation process
    //    }
    //}

    private async Task<Domain.Identity.ApplicationUser?> GetUserDetails(string userId)
    {
        try
        {
            // Try to get user details from client first
            var client = await _unitOfWork.Clients.GetByUserIdWithDetailsAsync(userId);
            if (client?.User != null)
            {
                return client.User;
            }

            // If not found in clients, try freelancers
            var freelancer = await _unitOfWork.Freelancers.GetByUserIdWithDetailsAsync(userId);
            if (freelancer?.User != null)
            {
                return freelancer.User;
            }

            _logger.LogWarning("User details not found for userId {UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user details for userId {UserId}", userId);
            return null;
        }
    }

    private async Task<string> GetReviewerName(string userId)
    {
        var user = await GetUserDetails(userId);
        return user?.FullName ?? "Anonymous User";
    }
}