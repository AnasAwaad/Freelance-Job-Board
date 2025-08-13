using MediatR;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using FreelanceJobBoard.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Reviews.Commands.QuickReview;

public class QuickReviewCommandHandler : IRequestHandler<QuickReviewCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<QuickReviewCommandHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public QuickReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<QuickReviewCommandHandler> logger,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<int> Handle(QuickReviewCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedAccessException("User must be authenticated to create a review.");

        // Basic validation
        if (request.Rating < 1 || request.Rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5 stars.");

        if (string.IsNullOrWhiteSpace(request.Comment))
            throw new ArgumentException("Comment is required.");

        // Check permissions
        var canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(request.JobId, currentUserId);
        if (!canReview)
            throw new InvalidOperationException("You cannot review this job.");

        var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedJobAsync(request.JobId, currentUserId);
        if (hasReviewed)
            throw new InvalidOperationException("You have already reviewed this job.");

        // Get job details for notification
        var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(request.JobId);
        if (job == null)
            throw new NotFoundException(nameof(Job), request.JobId.ToString());

        // Create review
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

        // Update average ratings
        await UpdateAverageRating(request.RevieweeId);

        // Send email notification to the reviewee about the new review
        await SendReviewNotification(review, job);

        // Send in-app notification about the review
        var reviewerName = await GetReviewerName(currentUserId);
        await _notificationService.NotifyReviewReceivedAsync(
            review.Id, 
            request.RevieweeId, 
            reviewerName, 
            job.Title ?? "Unknown Job", 
            request.Rating
        );

        _logger.LogInformation("Quick review created successfully. ReviewId: {ReviewId}, JobId: {JobId}, Rating: {Rating}",
            review.Id, request.JobId, request.Rating);

        return review.Id;
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

    private async Task SendReviewNotification(Review review, Job job)
    {
        try
        {
            // Get reviewer information
            var reviewer = await GetUserDetails(review.ReviewerId);
            var reviewee = await GetUserDetails(review.RevieweeId);

            if (reviewee?.Email == null)
            {
                _logger.LogWarning("Cannot send review notification - reviewee email not found for user {RevieweeId}", review.RevieweeId);
                return;
            }

            // Generate star rating display for email
            var starRating = new string('?', review.Rating) + new string('?', 5 - review.Rating);

            var emailData = new
            {
                RevieweeName = reviewee.FullName ?? "User",
                JobTitle = job.Title ?? "Unknown Job",
                StarRating = starRating,
                Rating = review.Rating,
                ReviewerName = reviewer?.FullName ?? "Anonymous",
                Comment = review.Comment ?? "No comment provided"
            };

            await _emailService.SendTemplateEmailAsync(
                reviewee.Email,
                "ReviewNotification",
                emailData);

            _logger.LogInformation("Review notification email sent to {RevieweeEmail} for review {ReviewId}", 
                reviewee.Email, review.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send review notification email for review {ReviewId}", review.Id);
            // Don't throw here to avoid breaking the review creation process
        }
    }

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