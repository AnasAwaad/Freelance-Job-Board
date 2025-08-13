using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FreelanceJobBoard.Application.Features.Reviews.Commands.CreateReview;
using FreelanceJobBoard.Application.Features.Reviews.Commands.QuickReview;
using FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewsByJob;
using FreelanceJobBoard.Application.Features.Reviews.Queries.GetReviewsByUser;
using FreelanceJobBoard.Application.Features.Reviews.Queries.GetPendingReviews;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using MediatR;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
using FreelanceJobBoard.Application.Features.Admin.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using ReviewDto = FreelanceJobBoard.Application.Features.Reviews.DTOs.ReviewDto;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IMediator mediator, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ILogger<ReviewsController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int jobId, string type)
    {
        try
        {
            _logger.LogInformation("Create Review method called for JobId: {JobId}, Type: {Type}", jobId, type);
            
            if (type != ReviewType.ClientToFreelancer && type != ReviewType.FreelancerToClient)
            {
                _logger.LogWarning("Invalid review type provided: {Type}", type);
                TempData["Error"] = "Invalid review type.";
                return RedirectToAction("Details", "Jobs", new { id = jobId });
            }

            var currentUserId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Create Review: User not authenticated");
                TempData["Error"] = "You must be logged in to submit a review.";
                return RedirectToAction("Login", "Auth");
            }

            _logger.LogInformation("Checking if user {UserId} can review job {JobId}", currentUserId, jobId);

            var canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(jobId, currentUserId);
            if (!canReview)
            {
                _logger.LogInformation("User {UserId} cannot review job {JobId}", currentUserId, jobId);
                
                var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
                if (job == null)
                {
                    _logger.LogWarning("Job {JobId} not found", jobId);
                    TempData["Error"] = "Job not found.";
                    return RedirectToAction("Index", "Jobs");
                }

                var isClient = job.Client?.UserId == currentUserId;
                var acceptedProposal = job.Proposals?.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
                var isAcceptedFreelancer = acceptedProposal?.Freelancer?.UserId == currentUserId;

                if (job.Status != JobStatus.Completed)
                {
                    TempData["Error"] = "You can only review completed jobs. Please wait for the job to be marked as completed by both parties.";
                }
                else if (!isClient && !isAcceptedFreelancer)
                {
                    TempData["Error"] = "You can only review jobs where you are either the client or the accepted freelancer.";
                }
                else
                {
                    TempData["Error"] = "You are not authorized to review this job.";
                }
                
                return RedirectToAction("Details", "Jobs", new { id = jobId });
            }

            var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedJobAsync(jobId, currentUserId);
            if (hasReviewed)
            {
                _logger.LogInformation("User {UserId} has already reviewed job {JobId}", currentUserId, jobId);
                TempData["Error"] = "You have already reviewed this job.";
                return RedirectToAction("Details", "Jobs", new { id = jobId });
            }

            var jobEntity = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (jobEntity == null)
            {
                _logger.LogWarning("Job entity {JobId} not found", jobId);
                TempData["Error"] = "Job not found.";
                return RedirectToAction("Index", "Jobs");
            }

            var viewModel = new CreateReviewViewModel
            {
                JobId = jobId,
                JobTitle = jobEntity.Title ?? "Unknown Job",
                ReviewType = type,
                IsVisible = true
            };

            if (type == ReviewType.ClientToFreelancer)
            {
                var acceptedProposal = jobEntity.Proposals?.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
                if (acceptedProposal?.Freelancer != null)
                {
                    viewModel.RevieweeId = acceptedProposal.Freelancer.UserId ?? string.Empty;
                    viewModel.RevieweeName = acceptedProposal.Freelancer.User?.FullName ?? "Freelancer";
                }
                else
                {
                    _logger.LogWarning("Unable to find freelancer for job {JobId}", jobId);
                    TempData["Error"] = "Unable to find the freelancer for this job.";
                    return RedirectToAction("Details", "Jobs", new { id = jobId });
                }
            }
            else if (type == ReviewType.FreelancerToClient)
            {
                if (jobEntity.Client?.User != null)
                {
                    viewModel.RevieweeId = jobEntity.Client.UserId ?? string.Empty;
                    viewModel.RevieweeName = jobEntity.Client.User.FullName ?? "Client";
                }
                else
                {
                    _logger.LogWarning("Unable to find client for job {JobId}", jobId);
                    TempData["Error"] = "Unable to find the client for this job.";
                    return RedirectToAction("Details", "Jobs", new { id = jobId });
                }
            }

            if (string.IsNullOrEmpty(viewModel.RevieweeId))
            {
                _logger.LogWarning("Unable to determine reviewee for job {JobId}", jobId);
                TempData["Error"] = "Unable to determine who to review for this job.";
                return RedirectToAction("Details", "Jobs", new { id = jobId });
            }

            _logger.LogInformation("Successfully prepared create review view model for job {JobId}", jobId);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading review form for job {JobId}, type {Type}", jobId, type);
            TempData["Error"] = "An error occurred while loading the review form: " + ex.Message;
            return RedirectToAction("Details", "Jobs", new { id = jobId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var command = new CreateReviewCommand
            {
                JobId = model.JobId,
                RevieweeId = model.RevieweeId,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewType = model.ReviewType,
                IsVisible = model.IsVisible,
                CommunicationRating = model.CommunicationRating,
                QualityRating = model.QualityRating,
                TimelinessRating = model.TimelinessRating,
                WouldRecommend = model.WouldRecommend,
                Tags = model.Tags
            };

            var reviewId = await _mediator.Send(command);

            TempData["Success"] = "Review submitted successfully! Thank you for your feedback.";
            return RedirectToAction("Details", "Jobs", new { id = model.JobId });
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["Error"] = ex.Message;
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review for JobId {JobId}, RevieweeId {RevieweeId}", model.JobId, model.RevieweeId);
            TempData["Error"] = "An error occurred while submitting your review. Please try again later.";
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> JobReviews(int jobId)
    {
        try
        {
            _logger.LogInformation("JobReviews method called for JobId: {JobId}", jobId);
            
            var query = new GetReviewsByJobQuery { JobId = jobId };
            var reviews = await _mediator.Send(query);

            var reviewsList = reviews?.ToList() ?? new List<ReviewDto>();
            
            _logger.LogInformation("Found {Count} reviews for JobId: {JobId}", reviewsList.Count, jobId);

            var job = await GetJobWithDetails(jobId);
            var jobTitle = job?.Title ?? "Unknown Job";

            var currentUserId = _currentUserService.UserId;
            var canReview = false;
            var pendingReviewType = string.Empty;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                try
                {
                    canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(jobId, currentUserId);
                    if (canReview)
                    {
                        var jobEntity = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
                        if (jobEntity != null)
                        {
                            var isClient = jobEntity.Client?.UserId == currentUserId;
                            if (isClient)
                            {
                                pendingReviewType = ReviewType.ClientToFreelancer;
                            }
                            else
                            {
                                var acceptedProposal = jobEntity.Proposals?.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
                                var isAcceptedFreelancer = acceptedProposal?.Freelancer?.UserId == currentUserId;
                                if (isAcceptedFreelancer)
                                {
                                    pendingReviewType = ReviewType.FreelancerToClient;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking review permissions for user {UserId} and job {JobId}", currentUserId, jobId);
                }
            }

            var viewModel = new JobReviewsViewModel
            {
                JobId = jobId,
                JobTitle = jobTitle,
                Reviews = reviewsList,
                CanCurrentUserReview = canReview,
                PendingReviewType = pendingReviewType
            };

            _logger.LogInformation("Successfully prepared JobReviews view model for JobId: {JobId}", jobId);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reviews for JobId {JobId}", jobId);
            TempData["Error"] = "An error occurred while loading reviews. Please try again later.";
            return RedirectToAction("Details", "Jobs", new { id = jobId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> UserReviews(string? userId = null)
    {
        try
        {
            _logger.LogInformation("UserReviews method called with userId: {UserId}", userId);
            
            userId = userId ?? _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserReviews: No userId found, user may not be authenticated");
                TempData["Error"] = "User not found. Please log in to view reviews.";
                return RedirectToAction("Login", "Auth");
            }

            _logger.LogInformation("Fetching reviews for user: {UserId}", userId);
            var query = new GetReviewsByUserQuery { UserId = userId };
            var reviewsSummary = await _mediator.Send(query);

            if (reviewsSummary == null)
            {
                _logger.LogWarning("No review summary returned for user: {UserId}", userId);
                var emptyViewModel = new UserReviewsViewModel
                {
                    UserId = userId,
                    UserName = "Unknown User",
                    AverageRating = 0,
                    TotalReviews = 0,
                    Reviews = new List<ReviewDto>(),
                    IsCurrentUser = userId == _currentUserService.UserId
                };
                return View(emptyViewModel);
            }

            var viewModel = new UserReviewsViewModel
            {
                UserId = userId,
                UserName = reviewsSummary.UserName,
                AverageRating = reviewsSummary.AverageRating,
                TotalReviews = reviewsSummary.TotalReviews,
                Reviews = reviewsSummary.RecentReviews?.ToList() ?? new List<ReviewDto>(),
                IsCurrentUser = userId == _currentUserService.UserId
            };

            _logger.LogInformation("Successfully loaded reviews for user: {UserId}, Total: {Count}", userId, viewModel.TotalReviews);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading reviews for user: {UserId}", userId);
            TempData["Error"] = "An error occurred while loading reviews: " + ex.Message;
            
            var errorViewModel = new UserReviewsViewModel
            {
                UserId = userId ?? string.Empty,
                UserName = "Error Loading User",
                AverageRating = 0,
                TotalReviews = 0,
                Reviews = new List<ReviewDto>(),
                IsCurrentUser = true
            };
            return View(errorViewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyReviews()
    {
        try
        {
            var currentUserId = _currentUserService.UserId ?? string.Empty;
            
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["Error"] = "You must be logged in to view your reviews.";
                return RedirectToAction("Login", "Auth");
            }
            
            return await UserReviews(currentUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reviews for MyReviews");
            TempData["Error"] = "An error occurred while loading your reviews. Please try again later.";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVisibility(int reviewId)
    {
        try
        {

            TempData["Error"] = "Review visibility toggle is not yet implemented.";
            return RedirectToAction("MyReviews");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling review visibility for ReviewId {ReviewId}", reviewId);
            TempData["Error"] = "An error occurred while updating the review. Please try again later.";
            return RedirectToAction("MyReviews");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        try
        {
            _logger.LogInformation("Pending Reviews method called");
            
            var currentUserId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("Pending Reviews: Current user ID is null or empty");
                TempData["Error"] = "You must be logged in to view pending reviews.";
                return RedirectToAction("Login", "Auth");
            }

            _logger.LogInformation("Fetching pending reviews for user: {UserId}", currentUserId);
            var query = new GetPendingReviewsQuery { UserId = currentUserId };
            var pendingReviews = await _mediator.Send(query);

            if (pendingReviews == null)
            {
                _logger.LogWarning("No pending reviews returned for user: {UserId}", currentUserId);
                var emptyViewModel = new PendingReviewsViewModel
                {
                    TotalPending = 0,
                    PendingReviews = new List<PendingReviewItemViewModel>()
                };
                return View(emptyViewModel);
            }

            var viewModel = new PendingReviewsViewModel
            {
                TotalPending = pendingReviews.TotalPending,
                PendingReviews = pendingReviews.PendingReviews?.Select(pr => new PendingReviewItemViewModel
                {
                    JobId = pr.JobId,
                    JobTitle = pr.JobTitle,
                    ReviewType = pr.ReviewType,
                    RevieweeName = pr.RevieweeName,
                    RevieweeId = pr.RevieweeId,
                    CompletedDate = pr.CompletedDate,
                    IsUrgent = pr.IsUrgent
                }).ToList() ?? new List<PendingReviewItemViewModel>()
            };

            _logger.LogInformation("Successfully loaded {Count} pending reviews for user: {UserId}", viewModel.TotalPending, currentUserId);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading pending reviews for user: {UserId}", _currentUserService.UserId);
            TempData["Error"] = "Unable to load pending reviews: " + ex.Message;
            
            var errorViewModel = new PendingReviewsViewModel
            {
                TotalPending = 0,
                PendingReviews = new List<PendingReviewItemViewModel>()
            };
            return View(errorViewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> QuickReview(int jobId, string reviewType, string revieweeId, string revieweeName)
    {
        try
        {
            var job = await GetJobWithDetails(jobId);
            if (job == null)
            {
                return Json(new { success = false, message = "Job not found." });
            }

            var viewModel = new QuickReviewViewModel
            {
                JobId = jobId,
                JobTitle = job.Title ?? "Unknown Job",
                RevieweeId = revieweeId,
                RevieweeName = revieweeName,
                ReviewType = reviewType
            };

            return PartialView("_QuickReviewModal", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading quick review form for JobId {JobId}", jobId);
            return Json(new { success = false, message = "Error loading review form." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuickReview(QuickReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please correct the validation errors: " + 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction("Pending");
        }

        try
        {
            var command = new QuickReviewCommand
            {
                JobId = model.JobId,
                RevieweeId = model.RevieweeId,
                ReviewType = model.ReviewType,
                Rating = model.Rating,
                Comment = model.Comment,
                IsVisible = model.IsVisible
            };

            var reviewId = await _mediator.Send(command);

            TempData["Success"] = "Review submitted successfully! Thank you for your feedback.";
            return RedirectToAction("Pending");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access while submitting quick review for JobId {JobId}, RevieweeId {RevieweeId}", model.JobId, model.RevieweeId);
            TempData["Error"] = "You are not authorized to submit this review.";
            return RedirectToAction("Pending");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation while submitting quick review for JobId {JobId}, RevieweeId {RevieweeId}", model.JobId, model.RevieweeId);
            TempData["Error"] = ex.Message;
            return RedirectToAction("Pending");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument while submitting quick review for JobId {JobId}, RevieweeId {RevieweeId}", model.JobId, model.RevieweeId);
            TempData["Error"] = ex.Message;
            return RedirectToAction("Pending");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting quick review for JobId {JobId}, RevieweeId {RevieweeId}", model.JobId, model.RevieweeId);
            TempData["Error"] = "An error occurred while submitting your review. Please try again later.";
            return RedirectToAction("Pending");
        }
    }

    private async Task<JobDetailsDto?> GetJobWithDetails(int jobId)
    {
        try
        {
            var query = new GetJobByIdQuery(jobId);
            var job = await _mediator.Send(query);
            return job;
        }
        catch
        {
            return null;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Debug()
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            var isAuthenticated = _currentUserService.IsAuthenticated;
            var userEmail = _currentUserService.UserEmail;

            var debugInfo = new
            {
                IsAuthenticated = isAuthenticated,
                CurrentUserId = currentUserId,
                UserEmail = userEmail,
                UserClaims = User?.Claims?.Select(c => new { c.Type, c.Value }).ToList(),
                Identity = new 
                {
                    Name = User?.Identity?.Name,
                    AuthenticationType = User?.Identity?.AuthenticationType,
                    IsAuthenticated = User?.Identity?.IsAuthenticated
                },
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Debug Info: {@DebugInfo}", debugInfo);

            ViewBag.DebugInfo = debugInfo;
            return View("Debug");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in debug action");
            ViewBag.DebugInfo = new { Error = ex.Message };
            return View("Debug");
        }
    }
}