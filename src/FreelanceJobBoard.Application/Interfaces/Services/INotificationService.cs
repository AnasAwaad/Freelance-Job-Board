using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Services;

public interface INotificationService
{
    // Core notification methods
    Task CreateNotificationAsync(string userId, string title, string message, int? templateId = null);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);

    // Enhanced notification creation with sender tracking
    Task CreateInteractionNotificationAsync(string recipientUserId, string? senderUserId, string type, 
        string title, string message, int? jobId = null, int? proposalId = null, int? contractId = null, 
        int? reviewId = null, object? additionalData = null);

    // Real-time notification methods
    Task SendRealTimeNotificationAsync(string userId, string title, string message, object? data = null);
    Task SendRealTimeNotificationToMultipleUsersAsync(IEnumerable<string> userIds, string title, string message, object? data = null);

    // Job-related notifications
    Task NotifyJobStatusChangeAsync(int jobId, string status, string? clientMessage = null);
    Task NotifyNewProposalAsync(int jobId, int proposalId);
    Task NotifyJobApprovalAsync(int jobId, bool isApproved, string? adminMessage = null);
    Task NotifyJobSubmittedForApprovalAsync(int jobId);
    Task NotifyJobCompletedAsync(int jobId, string clientId, string freelancerId, string jobTitle);
    Task NotifyJobUpdatedAsync(int jobId, string clientId);
    Task NotifyJobPostedAsync(int jobId, IEnumerable<string> interestedFreelancerIds);

    // Admin-specific job approval notifications
    Task NotifyAdminJobPendingApprovalAsync(int jobId, string clientId);
    Task NotifyClientJobApprovalResultAsync(int jobId, bool isApproved, string? adminMessage = null, string? adminUserId = null);
    Task NotifyAdminsJobRequiresReviewAsync(int jobId, string reason = "Job requires admin review");

    // Proposal-related notifications
    Task NotifyProposalStatusChangeAsync(int proposalId, string newStatus, string? feedback = null);
    Task NotifyProposalSubmittedAsync(int jobId, int proposalId, string freelancerId, string clientId);

    // Contract-related notifications
    Task NotifyContractCreatedAsync(int contractId, string clientId, string freelancerId, string jobTitle);
    Task NotifyContractStatusChangeAsync(int contractId, string newStatus, string userId, string counterpartyName);
    Task NotifyContractChangeRequestAsync(int contractId, string requesterId, string targetUserId, string jobTitle, string changeReason);
    Task NotifyContractChangeResponseAsync(int contractId, string responderId, string requesterId, string jobTitle, bool isApproved, string? responseNotes = null);
    Task NotifyContractCompletionRequestAsync(int contractId, string requesterId, string targetUserId, string jobTitle);

    // Review-related notifications
    Task NotifyReviewReceivedAsync(int reviewId, string revieweeId, string reviewerName, string jobTitle, int rating);
    Task NotifyReviewRequestAsync(string requesteeId, string requesterName, string jobTitle);
    Task NotifyReviewPendingAsync(string userId, string counterpartyName, string jobTitle, int daysRemaining);

    // Payment-related notifications
    Task NotifyPaymentReceivedAsync(string userId, decimal amount, string jobTitle);
    Task NotifyPaymentRequestedAsync(string payerId, string payeeName, decimal amount, string jobTitle);

    // User account notifications
    Task NotifyWelcomeMessageAsync(string userId, string userName);
    Task NotifyAccountVerificationAsync(string userId);
    Task NotifyPasswordChangedAsync(string userId);
    Task NotifyProfileUpdatedAsync(string userId, string updateType);

    // System notifications
    Task NotifySystemMaintenanceAsync(string message, DateTime? scheduledTime = null);
    Task NotifySystemUpdateAsync(string version, string features);

    // Deadline and reminder notifications
    Task NotifyDeadlineApproachingAsync(string userId, string itemType, string itemName, DateTime deadline);
    Task NotifyDeadlinePassedAsync(string userId, string itemType, string itemName, DateTime deadline);

    // Bulk operations
    Task DeleteNotificationAsync(int notificationId);
    Task DeleteOldNotificationsAsync(string userId, TimeSpan olderThan);

    // Notification analytics and management
    Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(string userId, string type);
    Task<bool> HasUnreadNotificationAsync(string userId, string type);
    Task MarkNotificationsByTypeAsReadAsync(string userId, string type);
    Task<IEnumerable<Notification>> GetNotificationsForJobAsync(int jobId);
    Task<IEnumerable<Notification>> GetNotificationsForContractAsync(int contractId);
}