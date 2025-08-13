using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    // Core notification methods
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false);
    Task<IEnumerable<Notification>> GetByTemplateIdAsync(int templateId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<IEnumerable<Notification>> GetRecentNotificationsAsync(string userId, int count = 10);
    Task DeleteOldNotificationsAsync(string userId, DateTime cutoffDate);
    Task<bool> HasUnreadNotificationsAsync(string userId);

    // Enhanced methods for user interaction tracking
    Task<IEnumerable<Notification>> GetByRecipientIdAsync(string recipientUserId, bool unreadOnly = false);
    Task<IEnumerable<Notification>> GetBySenderIdAsync(string senderUserId);
    Task<IEnumerable<Notification>> GetByTypeAsync(string userId, string type, bool unreadOnly = false);
    Task<IEnumerable<Notification>> GetInteractionHistoryAsync(string userId1, string userId2);
    
    // Entity-specific notification methods
    Task<IEnumerable<Notification>> GetByJobIdAsync(int jobId);
    Task<IEnumerable<Notification>> GetByProposalIdAsync(int proposalId);
    Task<IEnumerable<Notification>> GetByContractIdAsync(int contractId);
    Task<IEnumerable<Notification>> GetByReviewIdAsync(int reviewId);
    
    // Advanced filtering and analytics
    Task<IEnumerable<Notification>> GetNotificationsBetweenDatesAsync(string userId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<Notification>> GetUrgentNotificationsAsync(string userId);
    Task<IEnumerable<Notification>> GetExpiringNotificationsAsync(DateTime expiryThreshold);
    Task<Dictionary<string, int>> GetNotificationCountsByTypeAsync(string userId);
    
    // Bulk operations
    Task MarkNotificationsByTypeAsReadAsync(string userId, string type);
    Task MarkNotificationsByJobAsReadAsync(string userId, int jobId);
    Task MarkNotificationsByContractAsReadAsync(string userId, int contractId);
    Task DeleteNotificationsByTypeAsync(string userId, string type);
    Task DeleteExpiredNotificationsAsync();
    
    // Real-time and system methods
    Task<bool> HasUnreadNotificationOfTypeAsync(string userId, string type);
    Task<DateTime?> GetLastNotificationTimeAsync(string userId, string type);
    Task UpdateNotificationAsEmailSentAsync(int notificationId);
    Task<IEnumerable<Notification>> GetPendingEmailNotificationsAsync();
}