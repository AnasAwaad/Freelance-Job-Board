using System.ComponentModel.DataAnnotations;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Application.Features.Notifications.DTOs;

public class NotificationTemplateDto
{
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public string Color { get; set; } = null!;
    public bool IsUrgent { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public static class NotificationTemplates
{
    // Job-related notification templates
    public static NotificationTemplateDto JobCreated(string jobTitle, string clientName) => new()
    {
        Type = "job_created",
        Title = "New Job Posted",
        Message = $"New job '{jobTitle}' has been posted by {clientName}",
        Icon = "ki-briefcase",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto JobApproved(string jobTitle) => new()
    {
        Type = "job_approved",
        Title = "Job Approved",
        Message = $"Your job '{jobTitle}' has been approved and is now live",
        Icon = "ki-check-circle",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto JobRejected(string jobTitle, string? reason = null) => new()
    {
        Type = "job_rejected",
        Title = "Job Rejected",
        Message = $"Your job '{jobTitle}' was rejected" + (string.IsNullOrEmpty(reason) ? "" : $": {reason}"),
        Icon = "ki-cross-circle",
        Color = "danger",
        IsUrgent = true
    };

    public static NotificationTemplateDto JobUpdated(string jobTitle, string clientName) => new()
    {
        Type = "job_updated",
        Title = "Job Updated",
        Message = $"The job '{jobTitle}' by {clientName} has been updated",
        Icon = "ki-pencil",
        Color = "info",
        IsUrgent = false
    };

    // Admin-specific job approval templates
    public static NotificationTemplateDto JobPendingAdminApproval(string jobTitle, string clientName, decimal budgetMin, decimal budgetMax) => new()
    {
        Type = "job_pending_admin_approval",
        Title = "New Job Requires Approval",
        Message = $"Job '{jobTitle}' from {clientName} requires approval. Budget: ${budgetMin:N2} - ${budgetMax:N2}",
        Icon = "ki-notification-bing",
        Color = "warning",
        IsUrgent = true
    };

    public static NotificationTemplateDto JobRequiresAdminReview(string jobTitle, string reason) => new()
    {
        Type = "job_requires_admin_review",
        Title = "Job Requires Review",
        Message = $"Job '{jobTitle}' requires admin review: {reason}",
        Icon = "ki-warning-2",
        Color = "danger",
        IsUrgent = true
    };

    public static NotificationTemplateDto JobApprovedByAdmin(string jobTitle, string adminName) => new()
    {
        Type = "job_approved_by_admin",
        Title = "Job Approved",
        Message = $"Great news! Your job '{jobTitle}' has been approved by {adminName} and is now visible to freelancers.",
        Icon = "ki-check-circle",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto JobRejectedByAdmin(string jobTitle, string adminName, string? reason = null) => new()
    {
        Type = "job_rejected_by_admin",
        Title = "Job Requires Changes",
        Message = $"Your job '{jobTitle}' was not approved by {adminName}" + (string.IsNullOrEmpty(reason) ? ". Please review and resubmit." : $": {reason}"),
        Icon = "ki-cross-circle",
        Color = "danger",
        IsUrgent = true
    };

    // Proposal-related notification templates
    public static NotificationTemplateDto ProposalReceived(string jobTitle, string freelancerName, decimal bidAmount) => new()
    {
        Type = "proposal_received",
        Title = "New Proposal Received",
        Message = $"New proposal for '{jobTitle}' from {freelancerName} (${bidAmount:N2})",
        Icon = "ki-document",
        Color = "primary",
        IsUrgent = false
    };

    public static NotificationTemplateDto ProposalAccepted(string jobTitle) => new()
    {
        Type = "proposal_accepted",
        Title = "Proposal Accepted!",
        Message = $"Congratulations! Your proposal for '{jobTitle}' has been accepted",
        Icon = "ki-check-circle",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto ProposalRejected(string jobTitle, string? feedback = null) => new()
    {
        Type = "proposal_rejected",
        Title = "Proposal Not Selected",
        Message = $"Your proposal for '{jobTitle}' was not selected" + (string.IsNullOrEmpty(feedback) ? "" : $": {feedback}"),
        Icon = "ki-cross-circle",
        Color = "warning",
        IsUrgent = false
    };

    public static NotificationTemplateDto ProposalUnderReview(string jobTitle) => new()
    {
        Type = "proposal_under_review",
        Title = "Proposal Under Review",
        Message = $"Your proposal for '{jobTitle}' is now under review",
        Icon = "ki-time",
        Color = "info",
        IsUrgent = false
    };

    public static NotificationTemplateDto ProposalSubmitted(string jobTitle, string freelancerName) => new()
    {
        Type = "proposal_submitted",
        Title = "Proposal Submitted Successfully",
        Message = $"Your proposal for '{jobTitle}' has been submitted and is awaiting client review",
        Icon = "ki-check",
        Color = "info",
        IsUrgent = false
    };

    // Contract-related notification templates
    public static NotificationTemplateDto ContractCreated(string jobTitle, string counterpartyName) => new()
    {
        Type = "contract_created",
        Title = "Contract Created",
        Message = $"A new contract has been created for '{jobTitle}' with {counterpartyName}",
        Icon = "ki-handshake",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto ContractStatusChanged(string jobTitle, string newStatus, string counterpartyName) => new()
    {
        Type = "contract_status_changed",
        Title = "Contract Status Updated",
        Message = $"Contract for '{jobTitle}' with {counterpartyName} is now {newStatus}",
        Icon = "ki-handshake",
        Color = newStatus.ToLower() switch
        {
            "active" => "success",
            "completed" => "primary",
            "cancelled" => "danger",
            "pending" => "warning",
            _ => "info"
        },
        IsUrgent = newStatus.ToLower() == "cancelled"
    };

    public static NotificationTemplateDto ContractChangeRequested(string jobTitle, string requesterName, string changeReason) => new()
    {
        Type = "contract_change_requested",
        Title = "Contract Change Request",
        Message = $"{requesterName} has requested changes to the contract for '{jobTitle}': {changeReason}",
        Icon = "ki-pencil",
        Color = "warning",
        IsUrgent = true
    };

    public static NotificationTemplateDto ContractChangeApproved(string jobTitle, string approverName) => new()
    {
        Type = "contract_change_approved",
        Title = "Contract Changes Approved",
        Message = $"{approverName} has approved your contract changes for '{jobTitle}'",
        Icon = "ki-check-circle",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto ContractChangeRejected(string jobTitle, string rejecterName, string? reason = null) => new()
    {
        Type = "contract_change_rejected",
        Title = "Contract Changes Rejected",
        Message = $"{rejecterName} has rejected your contract changes for '{jobTitle}'" + (string.IsNullOrEmpty(reason) ? "" : $": {reason}"),
        Icon = "ki-cross-circle",
        Color = "danger",
        IsUrgent = false
    };

    public static NotificationTemplateDto ContractCompletionRequested(string jobTitle, string requesterName) => new()
    {
        Type = "contract_completion_requested",
        Title = "Contract Completion Request",
        Message = $"{requesterName} has requested completion of the contract for '{jobTitle}'",
        Icon = "ki-check",
        Color = "primary",
        IsUrgent = true
    };

    // Payment-related notification templates
    public static NotificationTemplateDto PaymentReceived(decimal amount, string jobTitle) => new()
    {
        Type = "payment_received",
        Title = "Payment Received",
        Message = $"You've received ${amount:N2} for '{jobTitle}'",
        Icon = "ki-dollar",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto PaymentRequested(decimal amount, string jobTitle, string requesterName) => new()
    {
        Type = "payment_requested",
        Title = "Payment Requested",
        Message = $"{requesterName} has requested ${amount:N2} for '{jobTitle}'",
        Icon = "ki-dollar",
        Color = "warning",
        IsUrgent = true
    };

    // Review-related notification templates
    public static NotificationTemplateDto ReviewReceived(string jobTitle, string reviewerName, int rating) => new()
    {
        Type = "review_received",
        Title = "New Review Received",
        Message = $"You received a {rating}-star review from {reviewerName} for '{jobTitle}'",
        Icon = "ki-star",
        Color = "primary",
        IsUrgent = false
    };

    public static NotificationTemplateDto ReviewRequested(string jobTitle, string requesterName) => new()
    {
        Type = "review_requested",
        Title = "Review Requested",
        Message = $"{requesterName} has requested a review for '{jobTitle}'",
        Icon = "ki-star",
        Color = "info",
        IsUrgent = false
    };

    public static NotificationTemplateDto ReviewReminder(string jobTitle, string counterpartyName, int daysRemaining) => new()
    {
        Type = "review_reminder",
        Title = "Review Reminder",
        Message = $"Don't forget to leave a review for {counterpartyName} on '{jobTitle}'. {daysRemaining} days remaining.",
        Icon = "ki-star",
        Color = "warning",
        IsUrgent = daysRemaining <= 1
    };

    // System notification templates
    public static NotificationTemplateDto WelcomeMessage(string userName) => new()
    {
        Type = "welcome",
        Title = "Welcome to FreelanceJobBoard!",
        Message = $"Hello {userName}! Welcome to our platform. Complete your profile to get started.",
        Icon = "ki-heart",
        Color = "primary",
        IsUrgent = false
    };

    public static NotificationTemplateDto AccountVerified() => new()
    {
        Type = "account_verified",
        Title = "Account Verified",
        Message = "Your account has been successfully verified. You now have full access to all features.",
        Icon = "ki-shield-tick",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto ProfileUpdated(string updateType) => new()
    {
        Type = "profile_updated",
        Title = "Profile Updated",
        Message = $"Your {updateType} has been updated successfully",
        Icon = "ki-profile-user",
        Color = "info",
        IsUrgent = false
    };

    public static NotificationTemplateDto SystemMaintenance(string message, DateTime? scheduledTime = null) => new()
    {
        Type = "system_maintenance",
        Title = "System Maintenance Notice",
        Message = scheduledTime.HasValue 
            ? $"{message} Scheduled for: {scheduledTime.Value:yyyy-MM-dd HH:mm} UTC"
            : message,
        Icon = "ki-setting-2",
        Color = "warning",
        IsUrgent = true
    };

    // Deadline notification templates
    public static NotificationTemplateDto DeadlineApproaching(string itemName, string itemType, int daysRemaining) => new()
    {
        Type = "deadline_approaching",
        Title = "Deadline Approaching",
        Message = $"Your {itemType} '{itemName}' is due in {daysRemaining} day{(daysRemaining == 1 ? "" : "s")}",
        Icon = "ki-time",
        Color = daysRemaining <= 1 ? "danger" : daysRemaining <= 3 ? "warning" : "info",
        IsUrgent = daysRemaining <= 1
    };

    public static NotificationTemplateDto DeadlinePassed(string itemName, string itemType) => new()
    {
        Type = "deadline_passed",
        Title = "Deadline Passed",
        Message = $"The deadline for your {itemType} '{itemName}' has passed",
        Icon = "ki-warning",
        Color = "danger",
        IsUrgent = true
    };

    // Message notification templates
    public static NotificationTemplateDto NewMessage(string senderName, string preview) => new()
    {
        Type = "new_message",
        Title = "New Message",
        Message = $"New message from {senderName}: {(preview.Length > 50 ? preview.Substring(0, 50) + "..." : preview)}",
        Icon = "ki-message-text",
        Color = "primary",
        IsUrgent = false
    };

    // Application-specific templates
    public static NotificationTemplateDto JobStatusChange(string jobTitle, string oldStatus, string newStatus) => new()
    {
        Type = "job_status_change",
        Title = "Job Status Updated",
        Message = $"Job '{jobTitle}' status changed from {oldStatus} to {newStatus}",
        Icon = "ki-briefcase",
        Color = newStatus.ToLower() switch
        {
            "completed" => "success",
            "cancelled" => "danger",
            "in progress" => "primary",
            _ => "info"
        },
        IsUrgent = false
    };

    public static NotificationTemplateDto FreelancerApplicationReceived(string jobTitle, string freelancerName) => new()
    {
        Type = "freelancer_application",
        Title = "New Application",
        Message = $"{freelancerName} has applied for your job '{jobTitle}'",
        Icon = "ki-profile-user",
        Color = "info",
        IsUrgent = false
    };

    // New interaction-focused templates
    public static NotificationTemplateDto JobInterestExpressed(string jobTitle, string freelancerName) => new()
    {
        Type = "job_interest_expressed",
        Title = "Freelancer Interested",
        Message = $"{freelancerName} has expressed interest in your job '{jobTitle}'",
        Icon = "ki-heart",
        Color = "info",
        IsUrgent = false
    };

    public static NotificationTemplateDto ClientProfileViewed(string clientName) => new()
    {
        Type = "client_profile_viewed",
        Title = "Profile Viewed",
        Message = $"{clientName} viewed your freelancer profile",
        Icon = "ki-eye",
        Color = "info",
        IsUrgent = false
    };

    public static NotificationTemplateDto FreelancerProfileViewed(string freelancerName) => new()
    {
        Type = "freelancer_profile_viewed",
        Title = "Profile Viewed",
        Message = $"{freelancerName} viewed your client profile",
        Icon = "ki-eye",
        Color = "info",
        IsUrgent = false
    };

    public static NotificationTemplateDto ContractMilestoneCompleted(string jobTitle, string milestoneName) => new()
    {
        Type = "contract_milestone_completed",
        Title = "Milestone Completed",
        Message = $"Milestone '{milestoneName}' for '{jobTitle}' has been completed",
        Icon = "ki-check-circle",
        Color = "success",
        IsUrgent = false
    };

    public static NotificationTemplateDto DisputeRaised(string jobTitle, string raiserName) => new()
    {
        Type = "dispute_raised",
        Title = "Dispute Raised",
        Message = $"{raiserName} has raised a dispute regarding '{jobTitle}'",
        Icon = "ki-warning",
        Color = "danger",
        IsUrgent = true
    };

    public static NotificationTemplateDto DisputeResolved(string jobTitle, string resolution) => new()
    {
        Type = "dispute_resolved",
        Title = "Dispute Resolved",
        Message = $"The dispute for '{jobTitle}' has been resolved: {resolution}",
        Icon = "ki-check-circle",
        Color = "success",
        IsUrgent = false
    };

    // Reminder templates
    public static NotificationTemplateDto GeneralReminder(string reminderType, string details) => new()
    {
        Type = "general_reminder",
        Title = $"{reminderType} Reminder",
        Message = details,
        Icon = "ki-notification-bing",
        Color = "info",
        IsUrgent = false
    };

    public static NotificationTemplateDto InactivityReminder(string userName, int daysSinceLogin) => new()
    {
        Type = "inactivity_reminder",
        Title = "We Miss You!",
        Message = $"Hi {userName}! You haven't logged in for {daysSinceLogin} days. Check out new opportunities waiting for you!",
        Icon = "ki-heart",
        Color = "primary",
        IsUrgent = false
    };
}

public class CreateNotificationRequest
{
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public int? TemplateId { get; set; }
}

public class TestNotificationRequest
{
    public string? Title { get; set; }
    public string? Message { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Type { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsUrgent { get; set; }
    public string? SenderUserId { get; set; }
    public string? SenderName { get; set; }
    public int? JobId { get; set; }
    public int? ProposalId { get; set; }
    public int? ContractId { get; set; }
    public int? ReviewId { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class NotificationListResponse
{
    public bool Success { get; set; }
    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public string? Message { get; set; }
}

public class NotificationCountResponse
{
    public bool Success { get; set; }
    public int Count { get; set; }
}

public class NotificationActionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? UnreadCount { get; set; }
}

public class NotificationAnalyticsDto
{
    public Dictionary<string, int> NotificationCountsByType { get; set; } = new();
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public DateTime? LastNotificationTime { get; set; }
    public List<string> MostActiveNotificationTypes { get; set; } = new();
}