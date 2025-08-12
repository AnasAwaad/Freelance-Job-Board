using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using FreelanceJobBoard.Domain.Identity;
using FreelanceJobBoard.Domain.Constants;
using Microsoft.AspNetCore.SignalR;
using FreelanceJobBoard.Infrastructure.Hubs;
using Microsoft.EntityFrameworkCore;
using FreelanceJobBoard.Application.Features.Notifications.DTOs;
using System.Text.Json;

namespace FreelanceJobBoard.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        IUnitOfWork unitOfWork, 
        IEmailService emailService, 
        ILogger<NotificationService> logger, 
        UserManager<ApplicationUser> userManager,
        IHubContext<NotificationHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    public async Task CreateNotificationAsync(string userId, string title, string message, int? templateId = null)
    {
        await CreateInteractionNotificationAsync(userId, null, "general", title, message, null, null, null, null, null);
    }

    public async Task CreateInteractionNotificationAsync(string recipientUserId, string? senderUserId, string type, 
        string title, string message, int? jobId = null, int? proposalId = null, int? contractId = null, 
        int? reviewId = null, object? additionalData = null)
    {
        try
        {
            // Debug logging
            _logger.LogWarning("?? [DEBUG] Creating notification - RecipientId: {RecipientId}, SenderUserId: {SenderId}, Type: {Type}, Title: {Title}", 
                recipientUserId, senderUserId ?? "System", type, title);

            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                UserId = recipientUserId, // Backward compatibility
                SenderUserId = senderUserId,
                Type = type,
                Title = title,
                Message = message,
                JobId = jobId,
                ProposalId = proposalId,
                ContractId = contractId,
                ReviewId = reviewId,
                TemplateId = null, // Remove hardcoded template ID
                NotificationTemplateId = null, // Remove hardcoded template ID to avoid FK constraint
                IsRead = false,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                Data = additionalData != null ? JsonSerializer.Serialize(additionalData) : null
            };

            _logger.LogWarning("?? [DEBUG] About to save notification to database...");
            
            await _unitOfWork.Notifications.CreateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning("?? [DEBUG] Notification saved! NotificationId: {NotificationId}", notification.Id);

            // Send real-time notification
            await SendRealTimeNotificationAsync(recipientUserId, title, message, new { 
                notificationId = notification.Id,
                type = type,
                senderUserId = senderUserId,
                jobId = jobId,
                proposalId = proposalId,
                contractId = contractId,
                reviewId = reviewId,
                timestamp = notification.CreatedOn,
                data = additionalData
            });

            _logger.LogInformation("Interaction notification created: {Type} from {SenderId} to {RecipientId}", 
                type, senderUserId ?? "System", recipientUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [CRITICAL] Failed to create interaction notification for user {UserId} - Type: {Type}, Title: {Title}", 
                recipientUserId, type, title);
            
            // Rethrow to ensure the calling code knows about the failure
            throw;
        }
    }

    // Enhanced method to create notifications with templates
    public async Task CreateNotificationWithTemplateAsync(string userId, NotificationTemplateDto template, object? additionalData = null)
    {
        try
        {
            var notification = new Notification
            {
                RecipientUserId = userId,
                UserId = userId, // Backward compatibility
                Type = template.Type,
                Title = template.Title,
                Message = template.Message,
                Icon = template.Icon,
                Color = template.Color,
                IsUrgent = template.IsUrgent,
                TemplateId = null, // Remove hardcoded template ID
                NotificationTemplateId = null, // Remove hardcoded template ID to avoid FK constraint
                IsRead = false,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                Data = additionalData != null ? JsonSerializer.Serialize(additionalData) : null
            };

            await _unitOfWork.Notifications.CreateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // Enhanced real-time notification with template data
            var notificationData = new
            {
                notificationId = notification.Id,
                title = template.Title,
                message = template.Message,
                type = template.Type,
                icon = template.Icon,
                color = template.Color,
                isUrgent = template.IsUrgent,
                timestamp = notification.CreatedOn,
                data = template.Data ?? additionalData
            };

            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveNotification", notificationData);

            // Update unread count
            var unreadCount = await GetUnreadCountAsync(userId);
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("UpdateUnreadCount", unreadCount);

            _logger.LogInformation("Template notification created for user {UserId}: {Title} (Type: {Type})", 
                userId, template.Title, template.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create template notification for user {UserId}", userId);
        }
    }

    // Enhanced CreateNotificationWithTemplateAsync that accepts sender information
    public async Task CreateNotificationWithTemplateAsync(string recipientUserId, string? senderUserId, NotificationTemplateDto template, object? additionalData = null)
    {
        try
        {
            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                UserId = recipientUserId, // Backward compatibility
                SenderUserId = senderUserId,
                Type = template.Type,
                Title = template.Title,
                Message = template.Message,
                Icon = template.Icon,
                Color = template.Color,
                IsUrgent = template.IsUrgent,
                TemplateId = null, // Remove hardcoded template ID
                NotificationTemplateId = null, // Remove hardcoded template ID to avoid FK constraint
                IsRead = false,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                Data = additionalData != null ? JsonSerializer.Serialize(additionalData) : null
            };

            await _unitOfWork.Notifications.CreateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // Enhanced real-time notification with template data
            var notificationData = new
            {
                notificationId = notification.Id,
                title = template.Title,
                message = template.Message,
                type = template.Type,
                icon = template.Icon,
                color = template.Color,
                isUrgent = template.IsUrgent,
                senderUserId = senderUserId,
                timestamp = notification.CreatedOn,
                data = template.Data ?? additionalData
            };

            await _hubContext.Clients.Group($"User_{recipientUserId}")
                .SendAsync("ReceiveNotification", notificationData);

            // Update unread count
            var unreadCount = await GetUnreadCountAsync(recipientUserId);
            await _hubContext.Clients.Group($"User_{recipientUserId}")
                .SendAsync("UpdateUnreadCount", unreadCount);

            _logger.LogInformation("Template notification created from {SenderId} to {RecipientId}: {Title} (Type: {Type})", 
                senderUserId ?? "System", recipientUserId, template.Title, template.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create template notification for user {UserId}", recipientUserId);
        }
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        return await _unitOfWork.Notifications.GetByUserIdAsync(userId, unreadOnly);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        try
        {
            await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
            
            // Get the notification to find the user and send real-time update
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification != null)
            {
                var unreadCount = await GetUnreadCountAsync(notification.RecipientUserId);
                
                await _hubContext.Clients.Group($"User_{notification.RecipientUserId}")
                    .SendAsync("NotificationMarkedAsRead", new { 
                        notificationId = notificationId,
                        unreadCount = unreadCount
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {NotificationId} as read", notificationId);
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        try
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            
            // Send real-time update
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("AllNotificationsMarkedAsRead", new { 
                    unreadCount = 0,
                    timestamp = DateTime.UtcNow
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task SendRealTimeNotificationAsync(string userId, string title, string message, object? data = null)
    {
        try
        {
            _logger.LogWarning("?? [DEBUG] Sending real-time notification to user {UserId}: {Title}", userId, title);

            var notificationData = new
            {
                title,
                message,
                timestamp = DateTime.UtcNow,
                data
            };

            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveNotification", notificationData);

            _logger.LogWarning("?? [DEBUG] Real-time notification sent to User_{UserId} group", userId);

            // Also update the unread count
            var unreadCount = await GetUnreadCountAsync(userId);
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("UpdateUnreadCount", unreadCount);

            _logger.LogInformation("Real-time notification sent to user {UserId}: {Title} (Unread count: {UnreadCount})", 
                userId, title, unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [CRITICAL] Failed to send real-time notification to user {UserId}: {Title}", userId, title);
        }
    }

    public async Task SendRealTimeNotificationToMultipleUsersAsync(IEnumerable<string> userIds, string title, string message, object? data = null)
    {
        try
        {
            var notificationData = new
            {
                title,
                message,
                timestamp = DateTime.UtcNow,
                data
            };

            var tasks = userIds.Select(async userId =>
            {
                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("ReceiveNotification", notificationData);

                var unreadCount = await GetUnreadCountAsync(userId);
                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("UpdateUnreadCount", unreadCount);
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation("Real-time notifications sent to {Count} users: {Title}", userIds.Count(), title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notifications to multiple users");
        }
    }

    // Job-related notifications using templates
    public async Task NotifyJobStatusChangeAsync(int jobId, string status, string? clientMessage = null)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job == null) return;

            var proposals = await _unitOfWork.Proposals.GetProposalsByJobIdAsync(jobId);
            
            foreach (var proposal in proposals)
            {
                if (proposal.Freelancer?.User != null)
                {
                    var template = status switch
                    {
                        ProposalStatus.Accepted => NotificationTemplates.ProposalAccepted(job.Title ?? "Job"),
                        ProposalStatus.Rejected => NotificationTemplates.ProposalRejected(job.Title ?? "Job", clientMessage),
                        ProposalStatus.UnderReview => NotificationTemplates.ProposalUnderReview(job.Title ?? "Job"),
                        _ => NotificationTemplates.JobStatusChange(job.Title ?? "Job", "Previous Status", status)
                    };

                    await CreateInteractionNotificationAsync(
                        proposal.Freelancer.UserId!, 
                        job.Client?.UserId, 
                        "job_status_change", 
                        template.Title, 
                        template.Message,
                        jobId, 
                        proposal.Id,
                        null,
                        null,
                        new { status = status, clientMessage = clientMessage }
                    );
                }
            }

            _logger.LogInformation("Job status change notifications sent for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job status change notifications for job {JobId}", jobId);
        }
    }

    public async Task NotifyJobUpdatedAsync(int jobId, string clientId)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job == null) return;

            // Get freelancers who have applied to this job
            var proposals = await _unitOfWork.Proposals.GetProposalsByJobIdAsync(jobId);
            var appliedFreelancerIds = proposals.Where(p => p.Freelancer?.UserId != null)
                                               .Select(p => p.Freelancer!.UserId!)
                                               .ToList();

            if (appliedFreelancerIds.Any())
            {
                var template = NotificationTemplates.JobStatusChange(job.Title ?? "Job", "Previous", "Updated");
                template.Title = "Job Updated";
                template.Message = $"The job '{job.Title}' you applied to has been updated by the client.";

                foreach (var freelancerId in appliedFreelancerIds)
                {
                    await CreateInteractionNotificationAsync(
                        freelancerId,
                        clientId,
                        "job_updated",
                        template.Title,
                        template.Message,
                        jobId,
                        null,
                        null,
                        null,
                        new { action = "job_updated" }
                    );
                }
            }

            _logger.LogInformation("Job update notifications sent for job {JobId} to {Count} freelancers", jobId, appliedFreelancerIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job update notifications for job {JobId}", jobId);
        }
    }

    public async Task NotifyJobPostedAsync(int jobId, IEnumerable<string> interestedFreelancerIds)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job == null) return;

            var template = NotificationTemplates.JobCreated(job.Title ?? "New Job", job.Client?.User?.FullName ?? "A client");

            foreach (var freelancerId in interestedFreelancerIds)
            {
                await CreateInteractionNotificationAsync(
                    freelancerId,
                    job.Client?.UserId,
                    "job_posted",
                    template.Title,
                    template.Message,
                    jobId,
                    null,
                    null,
                    null,
                    new { 
                        budgetMin = job.BudgetMin, 
                        budgetMax = job.BudgetMax,
                        deadline = job.Deadline
                    }
                );
            }

            _logger.LogInformation("Job posted notifications sent for job {JobId} to {Count} freelancers", jobId, interestedFreelancerIds.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job posted notifications for job {JobId}", jobId);
        }
    }

    public async Task NotifyNewProposalAsync(int jobId, int proposalId)
    {
        try
        {
            _logger.LogWarning("🔔 [DEBUG] NotifyNewProposalAsync called - JobId: {JobId}, ProposalId: {ProposalId}", jobId, proposalId);

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            _logger.LogWarning("🔔 [DEBUG] Job retrieved - Job: {Job}, Client: {Client}, User: {User}", 
                job?.Id, job?.Client?.Id, job?.Client?.User?.Id);

            var proposal = await _unitOfWork.Proposals.GetProposalWithDetailsAsync(proposalId);
            _logger.LogWarning("🔔 [DEBUG] Proposal retrieved - Proposal: {Proposal}, Freelancer: {Freelancer}, FreelancerUser: {FreelancerUser}", 
                proposal?.Id, proposal?.Freelancer?.Id, proposal?.Freelancer?.User?.Id);

            if (job?.Client?.User != null && proposal != null)
            {
                var freelancerName = proposal.Freelancer?.User?.FullName ?? "A freelancer";
                var template = NotificationTemplates.ProposalReceived(
                    job.Title ?? "Your Job", 
                    freelancerName, 
                    proposal.BidAmount
                );

                _logger.LogWarning("🔔 [DEBUG] About to create notification - ClientUserId: {ClientUserId}, Template: {Title}", 
                    job.Client.UserId, template.Title);

                await CreateInteractionNotificationAsync(
                    job.Client.UserId!,
                    proposal.Freelancer?.UserId,
                    "proposal_received",
                    template.Title,
                    template.Message,
                    jobId,
                    proposalId,
                    null,
                    null,
                    new { 
                        freelancerId = proposal.FreelancerId,
                        bidAmount = proposal.BidAmount
                    }
                );

                _logger.LogWarning("🔔 [DEBUG] Notification created successfully");
            }
            else
            {
                _logger.LogWarning("🔔 [DEBUG] Failed notification check - JobClientUser: {HasJobClientUser}, HasProposal: {HasProposal}", 
                    job?.Client?.User != null, proposal != null);
            }

            _logger.LogInformation("New proposal notifications sent for job {JobId}, proposal {ProposalId}", jobId, proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new proposal notifications for job {JobId}, proposal {ProposalId}", jobId, proposalId);
        }
    }

    public async Task NotifyProposalSubmittedAsync(int jobId, int proposalId, string freelancerId, string clientId)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            var proposal = await _unitOfWork.Proposals.GetProposalWithDetailsAsync(proposalId);
            
            if (job != null && proposal != null)
            {
                // Notify client about new proposal
                var clientTemplate = NotificationTemplates.ProposalReceived(
                    job.Title ?? "Your Job",
                    proposal.Freelancer?.User?.FullName ?? "A freelancer",
                    proposal.BidAmount
                );

                await CreateInteractionNotificationAsync(
                    clientId,
                    freelancerId,
                    "proposal_received",
                    clientTemplate.Title,
                    clientTemplate.Message,
                    jobId,
                    proposalId,
                    null,
                    null,
                    new { bidAmount = proposal.BidAmount, freelancerId = freelancerId }
                );

                // Notify freelancer about successful submission
                var freelancerTemplate = NotificationTemplates.ProposalUnderReview(job.Title ?? "Job");
                freelancerTemplate.Title = "Proposal Submitted Successfully";
                freelancerTemplate.Message = $"Your proposal for '{job.Title}' has been submitted and is now under review.";

                await CreateInteractionNotificationAsync(
                    freelancerId,
                    null, // System notification
                    "proposal_submitted",
                    freelancerTemplate.Title,
                    freelancerTemplate.Message,
                    jobId,
                    proposalId,
                    null,
                    null,
                    new { jobTitle = job.Title }
                );
            }

            _logger.LogInformation("Proposal submission notifications sent for proposal {ProposalId}", proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send proposal submission notifications for proposal {ProposalId}", proposalId);
        }
    }

    public async Task NotifyJobApprovalAsync(int jobId, bool isApproved, string? adminMessage = null)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            
            if (job?.Client?.User != null)
            {
                var template = isApproved 
                    ? NotificationTemplates.JobApproved(job.Title ?? "Your Job")
                    : NotificationTemplates.JobRejected(job.Title ?? "Your Job", adminMessage);

                await CreateInteractionNotificationAsync(
                    job.Client.UserId!,
                    null, // System/Admin notification
                    isApproved ? "job_approved" : "job_rejected",
                    template.Title,
                    template.Message,
                    jobId,
                    null,
                    null,
                    null,
                    new { 
                        isApproved = isApproved,
                        adminMessage = adminMessage
                    }
                );
            }

            _logger.LogInformation("Job approval notifications sent for job {JobId}, approved: {IsApproved}", jobId, isApproved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job approval notifications for job {JobId}", jobId);
        }
    }

    public async Task NotifyJobSubmittedForApprovalAsync(int jobId)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job == null) return;

            var adminUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
            
            foreach (var admin in adminUsers)
            {
                var template = NotificationTemplates.JobCreated(
                    job.Title ?? "New Job", 
                    job.Client?.User?.FullName ?? "Unknown Client"
                );
                template.Title = "New Job Needs Approval";
                template.Message = $"A new job '{job.Title}' from {job.Client?.User?.FullName ?? "a client"} requires approval. Budget: ${job.BudgetMin:N2} - ${job.BudgetMax:N2}";
                template.IsUrgent = true;
                template.Color = "warning";

                await CreateInteractionNotificationAsync(
                    admin.Id,
                    job.Client?.UserId,
                    "job_submitted_for_approval",
                    template.Title,
                    template.Message,
                    jobId,
                    null,
                    null,
                    null,
                    new { 
                        clientId = job.ClientId,
                        budgetMin = job.BudgetMin,
                        budgetMax = job.BudgetMax
                    }
                );
            }

            _logger.LogInformation("Job submission notifications sent to admins for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job submission notifications for job {JobId}", jobId);
        }
    }

    // Admin-specific job approval notifications
    public async Task NotifyAdminJobPendingApprovalAsync(int jobId, string clientId)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job == null) return;

            var adminUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
            var clientName = job.Client?.User?.FullName ?? "Unknown Client";

            var template = NotificationTemplates.JobPendingAdminApproval(
                job.Title ?? "New Job",
                clientName,
                job.BudgetMin,
                job.BudgetMax
            );

            foreach (var admin in adminUsers)
            {
                await CreateInteractionNotificationAsync(
                    admin.Id,
                    clientId,
                    "job_pending_admin_approval",
                    template.Title,
                    template.Message,
                    jobId,
                    null,
                    null,
                    null,
                    new { 
                        clientId = clientId,
                        clientName = clientName,
                        budgetMin = job.BudgetMin,
                        budgetMax = job.BudgetMax,
                        deadline = job.Deadline,
                        submittedAt = job.CreatedOn
                    }
                );
            }

            _logger.LogInformation("Admin job approval notifications sent for job {JobId} to {Count} admins", jobId, adminUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send admin job approval notifications for job {JobId}", jobId);
        }
    }

    public async Task NotifyClientJobApprovalResultAsync(int jobId, bool isApproved, string? adminMessage = null, string? adminUserId = null)
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job?.Client?.User == null) return;

            var adminUser = !string.IsNullOrEmpty(adminUserId) 
                ? await _userManager.FindByIdAsync(adminUserId) 
                : null;
            var adminName = adminUser?.FullName ?? "Admin Team";

            var template = isApproved 
                ? NotificationTemplates.JobApprovedByAdmin(job.Title ?? "Your Job", adminName)
                : NotificationTemplates.JobRejectedByAdmin(job.Title ?? "Your Job", adminName, adminMessage);

            await CreateInteractionNotificationAsync(
                job.Client.UserId!,
                adminUserId, // Admin who made the decision
                isApproved ? "job_approved_by_admin" : "job_rejected_by_admin",
                template.Title,
                template.Message,
                jobId,
                null,
                null,
                null,
                new { 
                    isApproved = isApproved,
                    adminMessage = adminMessage,
                    adminName = adminName,
                    reviewedAt = DateTime.UtcNow
                }
            );

            _logger.LogInformation("Client job approval result notification sent for job {JobId}, approved: {IsApproved}", jobId, isApproved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send client job approval result notification for job {JobId}", jobId);
        }
    }

    public async Task NotifyAdminsJobRequiresReviewAsync(int jobId, string reason = "Job requires admin review")
    {
        try
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job == null) return;

            var adminUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
            var template = NotificationTemplates.JobRequiresAdminReview(job.Title ?? "Job", reason);

            foreach (var admin in adminUsers)
            {
                await CreateInteractionNotificationAsync(
                    admin.Id,
                    job.Client?.UserId,
                    "job_requires_admin_review",
                    template.Title,
                    template.Message,
                    jobId,
                    null,
                    null,
                    null,
                    new { 
                        reason = reason,
                        flaggedAt = DateTime.UtcNow,
                        clientId = job.ClientId
                    }
                );
            }

            _logger.LogInformation("Admin review notifications sent for job {JobId} to {Count} admins", jobId, adminUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send admin review notifications for job {JobId}", jobId);
        }
    }

    // Proposal-related notifications
    public async Task NotifyProposalStatusChangeAsync(int proposalId, string newStatus, string? feedback = null)
    {
        try
        {
            var proposal = await _unitOfWork.Proposals.GetProposalWithDetailsAsync(proposalId);
            if (proposal?.Freelancer?.User != null)
            {
                var jobTitle = proposal.Job?.Title ?? "Job";
                var template = newStatus switch
                {
                    ProposalStatus.Accepted => NotificationTemplates.ProposalAccepted(jobTitle),
                    ProposalStatus.Rejected => NotificationTemplates.ProposalRejected(jobTitle, feedback),
                    ProposalStatus.UnderReview => NotificationTemplates.ProposalUnderReview(jobTitle),
                    _ => NotificationTemplates.ProposalUnderReview(jobTitle)
                };

                await CreateInteractionNotificationAsync(
                    proposal.Freelancer.UserId!,
                    proposal.Job?.Client?.UserId,
                    "proposal_status_change",
                    template.Title,
                    template.Message,
                    proposal.JobId,
                    proposalId,
                    null,
                    null,
                    new { 
                        status = newStatus,
                        feedback = feedback
                    }
                );
            }

            _logger.LogInformation("Proposal status change notification sent for proposal {ProposalId}", proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send proposal status change notification for proposal {ProposalId}", proposalId);
        }
    }

    // Contract-related notifications
    public async Task NotifyContractCreatedAsync(int contractId, string clientId, string freelancerId, string jobTitle)
    {
        try
        {
            var client = await _userManager.FindByIdAsync(clientId);
            var freelancer = await _userManager.FindByIdAsync(freelancerId);

            // Notify client
            var clientTemplate = NotificationTemplates.ContractCreated(jobTitle, freelancer?.FullName ?? "Freelancer");
            await CreateInteractionNotificationAsync(
                clientId,
                freelancerId,
                "contract_created",
                clientTemplate.Title,
                clientTemplate.Message,
                null,
                null,
                contractId,
                null,
                new { counterpartyId = freelancerId, jobTitle = jobTitle }
            );

            // Notify freelancer
            var freelancerTemplate = NotificationTemplates.ContractCreated(jobTitle, client?.FullName ?? "Client");
            await CreateInteractionNotificationAsync(
                freelancerId,
                clientId,
                "contract_created",
                freelancerTemplate.Title,
                freelancerTemplate.Message,
                null,
                null,
                contractId,
                null,
                new { counterpartyId = clientId, jobTitle = jobTitle }
            );

            _logger.LogInformation("Contract creation notifications sent for contract {ContractId}", contractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contract creation notifications for contract {ContractId}", contractId);
        }
    }

    public async Task NotifyContractStatusChangeAsync(int contractId, string newStatus, string userId, string counterpartyName)
    {
        try
        {
            var contract = await _unitOfWork.Contracts.GetContractWithDetailsAsync(contractId);
            if (contract == null) return;

            var jobTitle = contract.Proposal?.Job?.Title ?? "Project";
            var template = NotificationTemplates.ContractStatusChanged(jobTitle, newStatus, counterpartyName);

            // Determine who is sending this notification (the counterparty)
            var senderUserId = contract.Client?.UserId == userId ? contract.Freelancer?.UserId : contract.Client?.UserId;

            await CreateInteractionNotificationAsync(
                userId,
                senderUserId,
                "contract_status_change",
                template.Title,
                template.Message,
                contract.Proposal?.JobId,
                null,
                contractId,
                null,
                new { 
                    newStatus = newStatus,
                    jobTitle = jobTitle
                }
            );

            _logger.LogInformation("Contract status notification sent for contract {ContractId} to user {UserId}", contractId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contract status notification for contract {ContractId}", contractId);
        }
    }

    public async Task NotifyContractChangeRequestAsync(int contractId, string requesterId, string targetUserId, string jobTitle, string changeReason)
    {
        try
        {
            var requester = await _userManager.FindByIdAsync(requesterId);
            var requesterName = requester?.FullName ?? "Someone";

            var template = NotificationTemplates.ContractChangeRequested(jobTitle, requesterName, changeReason);

            await CreateInteractionNotificationAsync(
                targetUserId,
                requesterId,
                "contract_change_requested",
                template.Title,
                template.Message,
                null,
                null,
                contractId,
                null,
                new { 
                    changeReason = changeReason,
                    jobTitle = jobTitle
                }
            );

            _logger.LogInformation("Contract change request notification sent for contract {ContractId}", contractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contract change request notification for contract {ContractId}", contractId);
        }
    }

    public async Task NotifyContractChangeResponseAsync(int contractId, string responderId, string requesterId, string jobTitle, bool isApproved, string? responseNotes = null)
    {
        try
        {
            var responder = await _userManager.FindByIdAsync(responderId);
            var responderName = responder?.FullName ?? "Other party";

            var template = isApproved 
                ? NotificationTemplates.ContractChangeApproved(jobTitle, responderName)
                : NotificationTemplates.ContractChangeRejected(jobTitle, responderName, responseNotes);

            await CreateInteractionNotificationAsync(
                requesterId,
                responderId,
                isApproved ? "contract_change_approved" : "contract_change_rejected",
                template.Title,
                template.Message,
                null,
                null,
                contractId,
                null,
                new { 
                    isApproved = isApproved,
                    responseNotes = responseNotes,
                    jobTitle = jobTitle
                }
            );

            _logger.LogInformation("Contract change response notification sent for contract {ContractId}", contractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contract change response notification for contract {ContractId}", contractId);
        }
    }

    public async Task NotifyContractCompletionRequestAsync(int contractId, string requesterId, string targetUserId, string jobTitle)
    {
        try
        {
            var requester = await _userManager.FindByIdAsync(requesterId);
            var requesterName = requester?.FullName ?? "Someone";

            var template = NotificationTemplates.ContractCompletionRequested(jobTitle, requesterName);

            await CreateInteractionNotificationAsync(
                targetUserId,
                requesterId,
                "contract_completion_requested",
                template.Title,
                template.Message,
                null,
                null,
                contractId,
                null,
                new { jobTitle = jobTitle }
            );

            _logger.LogInformation("Contract completion request notification sent for contract {ContractId}", contractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contract completion request notification for contract {ContractId}", contractId);
        }
    }

    public async Task NotifyJobCompletedAsync(int jobId, string clientId, string freelancerId, string jobTitle)
    {
        try
        {
            // Notify client about completion
            var clientTemplate = NotificationTemplates.JobStatusChange(jobTitle, "In Progress", "Completed");
            clientTemplate.Message = $"Your job '{jobTitle}' has been completed. You can now leave a review for the freelancer.";
            
            await CreateInteractionNotificationAsync(
                clientId,
                freelancerId,
                "job_completed",
                clientTemplate.Title,
                clientTemplate.Message,
                jobId,
                null,
                null,
                null,
                new { 
                    action = "leave_review",
                    counterpartyId = freelancerId
                }
            );

            // Notify freelancer about completion
            var freelancerTemplate = NotificationTemplates.JobStatusChange(jobTitle, "In Progress", "Completed");
            freelancerTemplate.Message = $"The job '{jobTitle}' has been completed successfully. You can now leave a review for the client.";
            
            await CreateInteractionNotificationAsync(
                freelancerId,
                clientId,
                "job_completed",
                freelancerTemplate.Title,
                freelancerTemplate.Message,
                jobId,
                null,
                null,
                null,
                new { 
                    action = "leave_review",
                    counterpartyId = clientId
                }
            );

            _logger.LogInformation("Job completion notifications sent for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job completion notifications for job {JobId}", jobId);
        }
    }

    // Review-related notifications
    public async Task NotifyReviewReceivedAsync(int reviewId, string revieweeId, string reviewerName, string jobTitle, int rating)
    {
        try
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            var template = NotificationTemplates.ReviewReceived(jobTitle, reviewerName, rating);

            await CreateInteractionNotificationAsync(
                revieweeId,
                review?.ReviewerId,
                "review_received",
                template.Title,
                template.Message,
                review?.JobId,
                null,
                null,
                reviewId,
                new { 
                    reviewerName = reviewerName,
                    rating = rating,
                    jobTitle = jobTitle
                }
            );

            _logger.LogInformation("Review notification sent for review {ReviewId} to user {RevieweeId}", reviewId, revieweeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send review notification for review {ReviewId}", reviewId);
        }
    }

    public async Task NotifyReviewRequestAsync(string requesteeId, string requesterName, string jobTitle)
    {
        try
        {
            var template = NotificationTemplates.ReviewRequested(jobTitle, requesterName);

            await CreateInteractionNotificationAsync(
                requesteeId,
                null, // Could be enhanced to include requester ID
                "review_requested",
                template.Title,
                template.Message,
                null, // Could be enhanced to include job ID
                null,
                null,
                null,
                new { 
                    requesterName = requesterName,
                    jobTitle = jobTitle,
                    action = "submit_review"
                }
            );

            _logger.LogInformation("Review request notification sent to user {RequesteeId}", requesteeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send review request notification to user {RequesteeId}", requesteeId);
        }
    }

    public async Task NotifyReviewPendingAsync(string userId, string counterpartyName, string jobTitle, int daysRemaining)
    {
        try
        {
            var template = NotificationTemplates.DeadlineApproaching($"Review for {jobTitle}", "review", daysRemaining);
            template.Title = "Review Deadline Approaching";
            template.Message = $"You have {daysRemaining} day{(daysRemaining == 1 ? "" : "s")} left to leave a review for {counterpartyName} on '{jobTitle}'.";

            await CreateInteractionNotificationAsync(
                userId,
                null, // System notification
                "review_pending",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { 
                    counterpartyName = counterpartyName,
                    jobTitle = jobTitle,
                    daysRemaining = daysRemaining
                }
            );

            _logger.LogInformation("Review pending notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send review pending notification to user {UserId}", userId);
        }
    }

    // Payment-related notifications
    public async Task NotifyPaymentReceivedAsync(string userId, decimal amount, string jobTitle)
    {
        try
        {
            var template = NotificationTemplates.PaymentReceived(amount, jobTitle);

            await CreateInteractionNotificationAsync(
                userId,
                null, // Could be enhanced to include payer ID
                "payment_received",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { 
                    amount = amount,
                    jobTitle = jobTitle
                }
            );

            _logger.LogInformation("Payment notification sent to user {UserId} for amount {Amount}", userId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment notification to user {UserId}", userId);
        }
    }

    public async Task NotifyPaymentRequestedAsync(string payerId, string payeeName, decimal amount, string jobTitle)
    {
        try
        {
            var template = NotificationTemplates.PaymentRequested(amount, jobTitle, payeeName);

            await CreateInteractionNotificationAsync(
                payerId,
                null, // Could be enhanced to include payee ID
                "payment_requested",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { 
                    amount = amount,
                    jobTitle = jobTitle,
                    payeeName = payeeName
                }
            );

            _logger.LogInformation("Payment request notification sent to user {PayerId}", payerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment request notification to user {PayerId}", payerId);
        }
    }

    // User account notifications
    public async Task NotifyWelcomeMessageAsync(string userId, string userName)
    {
        try
        {
            var template = NotificationTemplates.WelcomeMessage(userName);

            await CreateInteractionNotificationAsync(
                userId,
                null, // System notification
                "welcome",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { 
                    userName = userName,
                    action = "complete_profile"
                }
            );

            _logger.LogInformation("Welcome notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome notification to user {UserId}", userId);
        }
    }

    public async Task NotifyAccountVerificationAsync(string userId)
    {
        try
        {
            var template = NotificationTemplates.AccountVerified();

            await CreateInteractionNotificationAsync(
                userId,
                null, // System notification
                "account_verified",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { verified = true }
            );

            _logger.LogInformation("Account verification notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send account verification notification to user {UserId}", userId);
        }
    }

    public async Task NotifyPasswordChangedAsync(string userId)
    {
        try
        {
            var template = NotificationTemplates.ProfileUpdated("password");
            template.Title = "Password Changed";
            template.Message = "Your account password has been changed successfully. If you didn't make this change, please contact support immediately.";
            template.IsUrgent = true;
            template.Color = "warning";

            await CreateInteractionNotificationAsync(
                userId,
                null, // System notification
                "password_changed",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { action = "password_changed" }
            );

            _logger.LogInformation("Password change notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password change notification to user {UserId}", userId);
        }
    }

    public async Task NotifyProfileUpdatedAsync(string userId, string updateType)
    {
        try
        {
            var template = NotificationTemplates.ProfileUpdated(updateType);

            await CreateInteractionNotificationAsync(
                userId,
                null, // System notification
                "profile_updated",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { updateType = updateType }
            );

            _logger.LogInformation("Profile update notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send profile update notification to user {UserId}", userId);
        }
    }

    // System notifications
    public async Task NotifySystemMaintenanceAsync(string message, DateTime? scheduledTime = null)
    {
        try
        {
            var template = NotificationTemplates.SystemMaintenance(message, scheduledTime);

            // Get all active users
            var allUsers = await _userManager.Users.Where(u => u.LockoutEnd == null || u.LockoutEnd < DateTime.UtcNow).ToListAsync();

            foreach (var user in allUsers)
            {
                await CreateInteractionNotificationAsync(
                    user.Id,
                    null, // System notification
                    "system_maintenance",
                    template.Title,
                    template.Message,
                    null,
                    null,
                    null,
                    null,
                    new { 
                        scheduledTime = scheduledTime,
                        maintenanceMessage = message
                    }
                );
            }

            _logger.LogInformation("System maintenance notifications sent to {Count} users", allUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system maintenance notifications");
        }
    }

    public async Task NotifySystemUpdateAsync(string version, string features)
    {
        try
        {
            var template = NotificationTemplates.SystemMaintenance($"System updated to version {version}!");
            template.Title = $"System Update - Version {version}";
            template.Message = $"Our platform has been updated to version {version}!\n\nNew features:\n{features}";
            template.Type = "system_update";
            template.Color = "primary";
            template.IsUrgent = false;

            // Send to all active users
            var allUsers = await _userManager.Users.Where(u => u.LockoutEnd == null || u.LockoutEnd < DateTime.UtcNow).ToListAsync();

            foreach (var user in allUsers)
            {
                await CreateInteractionNotificationAsync(
                    user.Id,
                    null, // System notification
                    "system_update",
                    template.Title,
                    template.Message,
                    null,
                    null,
                    null,
                    null,
                    new { 
                        version = version,
                        features = features
                    }
                );
            }

            _logger.LogInformation("System update notifications sent to {Count} users", allUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system update notifications");
        }
    }

    // Deadline and reminder notifications
    public async Task NotifyDeadlineApproachingAsync(string userId, string itemType, string itemName, DateTime deadline)
    {
        try
        {
            var timeRemaining = deadline - DateTime.UtcNow;
            var daysRemaining = (int)timeRemaining.TotalDays;
            
            var template = NotificationTemplates.DeadlineApproaching(itemName, itemType, daysRemaining);

            await CreateInteractionNotificationAsync(
                userId,
                null, // System notification
                "deadline_approaching",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { 
                    itemType = itemType,
                    itemName = itemName,
                    deadline = deadline,
                    daysRemaining = daysRemaining
                }
            );

            _logger.LogInformation("Deadline approaching notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send deadline approaching notification to user {UserId}", userId);
        }
    }

    public async Task NotifyDeadlinePassedAsync(string userId, string itemType, string itemName, DateTime deadline)
    {
        try
        {
            var template = NotificationTemplates.DeadlinePassed(itemName, itemType);

            await CreateInteractionNotificationAsync(
                userId,
                null, // System notification
                "deadline_passed",
                template.Title,
                template.Message,
                null,
                null,
                null,
                null,
                new { 
                    itemType = itemType,
                    itemName = itemName,
                    deadline = deadline,
                    overdue = true
                }
            );

            _logger.LogInformation("Deadline passed notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send deadline passed notification to user {UserId}", userId);
        }
    }

    // Bulk operations
    public async Task DeleteNotificationAsync(int notificationId)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification != null)
            {
                _unitOfWork.Notifications.Delete(notification);
                await _unitOfWork.SaveChangesAsync();

                // Send real-time update
                await _hubContext.Clients.Group($"User_{notification.RecipientUserId}")
                    .SendAsync("NotificationDeleted", new { 
                        notificationId = notificationId,
                        timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("Notification {NotificationId} deleted", notificationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete notification {NotificationId}", notificationId);
        }
    }

    public async Task DeleteOldNotificationsAsync(string userId, TimeSpan olderThan)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.Subtract(olderThan);
            await _unitOfWork.Notifications.DeleteOldNotificationsAsync(userId, cutoffDate);

            // Send real-time update
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("OldNotificationsDeleted", new { 
                    cutoffDate = cutoffDate,
                    timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("Old notifications deleted for user {UserId} older than {CutoffDate}", userId, cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old notifications for user {UserId}", userId);
        }
    }

    // New methods for notification analytics and management
    public async Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string userId, string type)
    {
        try
        {
            var allNotifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId, false);
            return allNotifications.Where(n => n.Type == type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications by type for user {UserId}", userId);
            return Enumerable.Empty<Notification>();
        }
    }

    public async Task<bool> HasUnreadNotificationAsync(string userId, string type)
    {
        try
        {
            var unreadNotifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId, true);
            return unreadNotifications.Any(n => n.Type == type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check unread notifications for user {UserId}", userId);
            return false;
        }
    }

    public async Task MarkNotificationsByTypeAsReadAsync(string userId, string type)
    {
        try
        {
            var allNotifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId, false);
            var notificationsToMark = allNotifications.Where(n => n.Type == type && !n.IsRead).ToList();

            foreach (var notification in notificationsToMark)
            {
                await _unitOfWork.Notifications.MarkAsReadAsync(notification.Id);
            }

            if (notificationsToMark.Any())
            {
                // Send real-time update
                var unreadCount = await GetUnreadCountAsync(userId);
                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("NotificationsByTypeMarkedAsRead", new { 
                        type = type,
                        count = notificationsToMark.Count,
                        unreadCount = unreadCount,
                        timestamp = DateTime.UtcNow
                    });
            }

            _logger.LogInformation("Marked {Count} notifications of type {Type} as read for user {UserId}", 
                notificationsToMark.Count, type, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notifications by type as read for user {UserId}", userId);
        }
    }

    public async Task<IEnumerable<Notification>> GetNotificationsForJobAsync(int jobId)
    {
        try
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            return allNotifications.Where(n => n.JobId == jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications for job {JobId}", jobId);
            return Enumerable.Empty<Notification>();
        }
    }

    public async Task<IEnumerable<Notification>> GetNotificationsForContractAsync(int contractId)
    {
        try
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            return allNotifications.Where(n => n.ContractId == contractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications for contract {ContractId}", contractId);
            return Enumerable.Empty<Notification>();
        }
    }
}