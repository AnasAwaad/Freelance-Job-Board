using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Services;

public interface INotificationService
{
    // Core notification methods
    Task CreateNotificationAsync(string userId, string title, string message, int? templateId = null);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId);

    // Job-related notifications
    Task NotifyJobStatusChangeAsync(int jobId, string status, string? clientMessage = null);
    Task NotifyNewProposalAsync(int jobId, int proposalId);
    Task NotifyJobApprovalAsync(int jobId, bool isApproved, string? adminMessage = null);
    Task NotifyJobSubmittedForApprovalAsync(int jobId);
    Task NotifyJobCompletedAsync(int jobId, string clientId, string freelancerId, string jobTitle);

    // Review-related notifications
    Task NotifyReviewReceivedAsync(int reviewId, string revieweeId, string reviewerName, string jobTitle, int rating);

    // Contract-related notifications
    Task NotifyContractStatusChangeAsync(int contractId, string newStatus, string userId, string counterpartyName);

    // Payment-related notifications
    Task NotifyPaymentReceivedAsync(string userId, decimal amount, string jobTitle);
}