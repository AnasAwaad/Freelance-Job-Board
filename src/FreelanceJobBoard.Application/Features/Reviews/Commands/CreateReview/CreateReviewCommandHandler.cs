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
        _logger.LogInformation("? Starting review creation | JobId={JobId}, ReviewerId={ReviewerId}, RevieweeId={RevieweeId}, Rating={Rating}", 
            request.JobId, currentUserId, request.RevieweeId, request.Rating);

        if (string.IsNullOrEmpty(currentUserId))
        {
            _logger.LogWarning("? Unauthenticated review attempt | JobId={JobId}", request.JobId);
            throw new UnauthorizedAccessException("User must be authenticated to create a review.");
        }

        _logger.LogDebug("?? Validating review permissions | JobId={JobId}, ReviewerId={ReviewerId}", 
            request.JobId, currentUserId);

        var canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(request.JobId, currentUserId);
        if (!canReview)
        {
            _logger.LogWarning("? Review permission denied | JobId={JobId}, ReviewerId={ReviewerId}", 
                request.JobId, currentUserId);
            throw new InvalidOperationException("You cannot review this job. The job must be completed and you must be either the client or the accepted freelancer.");
        }

        var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedJobAsync(request.JobId, currentUserId);
        if (hasReviewed)
        {
            _logger.LogWarning("? Duplicate review attempt | JobId={JobId}, ReviewerId={ReviewerId}", 
                request.JobId, currentUserId);
            throw new InvalidOperationException("You have already reviewed this job.");
        }

        _logger.LogDebug("? Review validation passed | JobId={JobId}, ReviewerId={ReviewerId}", 
            request.JobId, currentUserId);

        var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(request.JobId);
        if (job == null)
        {
            _logger.LogWarning("? Job not found for review | JobId={JobId}, ReviewerId={ReviewerId}", 
                request.JobId, currentUserId);
            throw new NotFoundException(nameof(Job), request.JobId.ToString());
        }

        _logger.LogDebug("? Job found for review | JobId={JobId}, JobTitle={JobTitle}, JobStatus={JobStatus}", 
            job.Id, job.Title, job.Status);

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

        _logger.LogDebug("?? Creating review record | JobId={JobId}, ReviewType={ReviewType}, Rating={Rating}/5", 
            request.JobId, request.ReviewType, request.Rating);

        await _unitOfWork.Reviews.CreateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogDebug("?? Review saved successfully | ReviewId={ReviewId}, JobId={JobId}", 
            review.Id, request.JobId);

        try
        {
            _logger.LogDebug("?? Updating average rating | RevieweeId={RevieweeId}", request.RevieweeId);
            await UpdateAverageRating(request.RevieweeId);
            _logger.LogDebug("? Average rating updated successfully | RevieweeId={RevieweeId}", request.RevieweeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Failed to update average rating | RevieweeId={RevieweeId}", request.RevieweeId);
        }

        //await SendReviewNotification(review, job);

        try
        {
            var reviewerName = await GetReviewerName(currentUserId);
            _logger.LogDebug("?? Sending review notification | ReviewId={ReviewId}, RevieweeId={RevieweeId}", 
                review.Id, request.RevieweeId);
            
            await _notificationService.NotifyReviewReceivedAsync(
                review.Id, 
                request.RevieweeId, 
                reviewerName, 
                job.Title ?? "Unknown Job", 
                request.Rating
            );
            
            _logger.LogDebug("? Review notification sent successfully | ReviewId={ReviewId}", review.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Failed to send review notification | ReviewId={ReviewId}", review.Id);
        }

        _logger.LogInformation("? Review created successfully | ReviewId={ReviewId}, JobId={JobId}, ReviewerId={ReviewerId}, Rating={Rating}/5",
            review.Id, request.JobId, currentUserId, request.Rating);

        return review.Id;
    }

    private async Task ValidateRevieweeAsync(CreateReviewCommand request, string currentUserId, Job job)
    {
        _logger.LogDebug("?? Validating reviewee permissions | JobId={JobId}, ReviewType={ReviewType}, RevieweeId={RevieweeId}", 
            request.JobId, request.ReviewType, request.RevieweeId);

        var isClient = job.Client?.UserId == currentUserId;
        
        var acceptedProposal = job.Proposals?.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
        var isFreelancer = acceptedProposal?.Freelancer?.UserId == currentUserId;

        _logger.LogDebug("?? User role validation | IsClient={IsClient}, IsFreelancer={IsFreelancer}, AcceptedProposalExists={ProposalExists}", 
            isClient, isFreelancer, acceptedProposal != null);

        if (request.ReviewType == ReviewType.ClientToFreelancer)
        {
            if (!isClient)
            {
                _logger.LogWarning("? Unauthorized ClientToFreelancer review attempt | ReviewerId={ReviewerId}, JobId={JobId}", 
                    currentUserId, request.JobId);
                throw new UnauthorizedAccessException("Only the client can create a ClientToFreelancer review.");
            }
            
            if (acceptedProposal?.Freelancer?.UserId != request.RevieweeId)
            {
                _logger.LogWarning("? Invalid reviewee for ClientToFreelancer review | ExpectedFreelancerId={ExpectedId}, ProvidedRevieweeId={ProvidedId}", 
                    acceptedProposal?.Freelancer?.UserId, request.RevieweeId);
                throw new ArgumentException("Reviewee must be the accepted freelancer for this job.");
            }
        }
        else if (request.ReviewType == ReviewType.FreelancerToClient)
        {
            if (!isFreelancer)
            {
                _logger.LogWarning("? Unauthorized FreelancerToClient review attempt | ReviewerId={ReviewerId}, JobId={JobId}", 
                    currentUserId, request.JobId);
                throw new UnauthorizedAccessException("Only the accepted freelancer can create a FreelancerToClient review.");
            }
            
            if (job.Client?.UserId != request.RevieweeId)
            {
                _logger.LogWarning("? Invalid reviewee for FreelancerToClient review | ExpectedClientId={ExpectedId}, ProvidedRevieweeId={ProvidedId}", 
                    job.Client?.UserId, request.RevieweeId);
                throw new ArgumentException("Reviewee must be the client for this job.");
            }
        }
        else
        {
            _logger.LogWarning("? Invalid review type | ReviewType={ReviewType}, JobId={JobId}", 
                request.ReviewType, request.JobId);
            throw new ArgumentException("Invalid review type.");
        }

        _logger.LogDebug("? Reviewee validation passed | ReviewType={ReviewType}, RevieweeId={RevieweeId}", 
            request.ReviewType, request.RevieweeId);
    }

    private async Task UpdateAverageRating(string userId)
    {
        try
        {
            var averageRating = await _unitOfWork.Reviews.GetAverageRatingByRevieweeIdAsync(userId);
            var totalReviews = await _unitOfWork.Reviews.GetTotalReviewCountByRevieweeIdAsync(userId);

            _logger.LogDebug("?? Rating statistics | UserId={UserId}, AverageRating={AverageRating}, TotalReviews={TotalReviews}", 
                userId, averageRating, totalReviews);

            var client = await _unitOfWork.Clients.GetByUserIdAsync(userId);
            if (client != null)
            {
                client.AverageRating = averageRating;
                client.TotalReviews = totalReviews;
                _unitOfWork.Clients.Update(client);
                _logger.LogDebug("? Client rating updated | ClientId={ClientId}, NewAverageRating={AverageRating}", 
                    client.Id, averageRating);
            }
            else
            {
                var freelancer = await _unitOfWork.Freelancers.GetByUserIdAsync(userId);
                if (freelancer != null)
                {
                    freelancer.AverageRating = averageRating;
                    freelancer.TotalReviews = totalReviews;
                    _unitOfWork.Freelancers.Update(freelancer);
                    _logger.LogDebug("? Freelancer rating updated | FreelancerId={FreelancerId}, NewAverageRating={AverageRating}", 
                        freelancer.Id, averageRating);
                }
                else
                {
                    _logger.LogWarning("?? User not found in clients or freelancers | UserId={UserId}", userId);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Failed to update average rating | UserId={UserId}", userId);
            throw;
        }
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

            _logger.LogWarning("?? User details not found | UserId={UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error retrieving user details | UserId={UserId}", userId);
            return null;
        }
    }

    private async Task<string> GetReviewerName(string userId)
    {
        var user = await GetUserDetails(userId);
        var name = user?.FullName ?? "Anonymous User";
        _logger.LogDebug("?? Retrieved reviewer name | UserId={UserId}, Name={Name}", userId, name);
        return name;
    }
}