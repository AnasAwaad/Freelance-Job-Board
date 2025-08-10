using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using FreelanceJobBoard.Domain.Identity;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Infrastructure.Services;

internal class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<NotificationService> logger, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task CreateNotificationAsync(string userId, string title, string message, int? templateId = null)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                TemplateId = templateId ?? 1, 
                NotificationTemplateId = templateId ?? 1,
                IsRead = false,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.CreateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Notification created for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification for user {UserId}", userId);
            throw;
        }
    }

    public async Task NotifyJobStatusChangeAsync(int jobId, string status, string? clientMessage = null)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
            if (job == null) return;

            var proposals = await _unitOfWork.Proposals.GetProposalsByJobIdAsync(jobId);
            
            foreach (var proposal in proposals)
            {
                if (proposal.Freelancer?.User?.Email != null)
                {
                    await _emailService.SendJobUpdateNotificationAsync(
                        proposal.Freelancer.User.Email,
                        job.Title ?? "Job",
                        status,
                        clientMessage
                    );

                    await CreateNotificationAsync(
                        proposal.Freelancer.UserId!,
                        $"Job Status Update: {job.Title}",
                        $"The status of your application has been updated to: {status}" +
                        (string.IsNullOrEmpty(clientMessage) ? "" : $"\n\nClient message: {clientMessage}")
                    );
                }
            }

            _logger.LogInformation("Job status change notifications sent for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job status change notifications for job {JobId}", jobId);
            throw;
        }
    }

    public async Task NotifyNewProposalAsync(int jobId, int proposalId)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
            var proposal = await _unitOfWork.Proposals.GetProposalWithDetailsAsync(proposalId);

            if (job?.Client?.User?.Email != null && proposal != null)
            {
                await _emailService.SendNewProposalNotificationAsync(
                    job.Client.User.Email,
                    job.Title ?? "Your Job",
                    proposal.Freelancer?.User?.FullName ?? "Unknown Freelancer",
                    proposal.BidAmount
                );

                await CreateNotificationAsync(
                    job.Client.UserId!,
                    $"New Proposal for: {job.Title}",
                    $"You received a new proposal from {proposal.Freelancer?.User?.FullName ?? "a freelancer"} " +
                    $"with a bid amount of ${proposal.BidAmount:N2}"
                );
            }

            _logger.LogInformation("New proposal notifications sent for job {JobId}, proposal {ProposalId}", jobId, proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new proposal notifications for job {JobId}, proposal {ProposalId}", jobId, proposalId);
            throw;
        }
    }

    public async Task NotifyJobApprovalAsync(int jobId, bool isApproved, string? adminMessage = null)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetByIdAsync(jobId);
            
            if (job?.Client?.User?.Email != null)
            {
                await _emailService.SendJobApprovalNotificationAsync(
                    job.Client.User.Email,
                    job.Title ?? "Your Job",
                    isApproved,
                    adminMessage
                );

                var status = isApproved ? "approved" : "rejected";
                await CreateNotificationAsync(
                    job.Client.UserId!,
                    $"Job {status}: {job.Title}",
                    $"Your job posting has been {status} by the admin team." +
                    (string.IsNullOrEmpty(adminMessage) ? "" : $"\n\nAdmin message: {adminMessage}")
                );
            }

            _logger.LogInformation("Job approval notifications sent for job {JobId}, approved: {IsApproved}", jobId, isApproved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job approval notifications for job {JobId}", jobId);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        return await _unitOfWork.Notifications.GetByUserIdAsync(userId, unreadOnly);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
    }

    public async Task NotifyJobSubmittedForApprovalAsync(int jobId)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job == null) return;

            // Get all admin users
            var adminUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
            
            foreach (var admin in adminUsers)
            {
                if (!string.IsNullOrEmpty(admin.Email))
                {
                    await _emailService.SendJobSubmissionNotificationAsync(
                        admin.Email,
                        job.Title ?? "New Job",
                        job.Client?.User?.FullName ?? "Unknown Client",
                        job.BudgetMin,
                        job.BudgetMax
                    );

                    await CreateNotificationAsync(
                        admin.Id,
                        $"New Job Needs Approval: {job.Title}",
                        $"A new job posting from {job.Client?.User?.FullName ?? "a client"} " +
                        $"requires your approval. Budget: ${job.BudgetMin:N2} - ${job.BudgetMax:N2}"
                    );
                }
            }

            _logger.LogInformation("Job submission notifications sent to admins for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job submission notifications for job {JobId}", jobId);
            throw;
        }
    }
}