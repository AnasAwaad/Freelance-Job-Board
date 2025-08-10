using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string title, string message, int? templateId = null);
    Task NotifyJobStatusChangeAsync(int jobId, string status, string? clientMessage = null);
    Task NotifyNewProposalAsync(int jobId, int proposalId);
    Task NotifyJobApprovalAsync(int jobId, bool isApproved, string? adminMessage = null);
    Task NotifyJobSubmittedForApprovalAsync(int jobId);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId);
}