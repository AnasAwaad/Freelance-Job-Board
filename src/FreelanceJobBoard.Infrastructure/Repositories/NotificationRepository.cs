using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

internal class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context, ILogger<GenericRepository<Notification>>? logger = null) : base(context, logger)
    {
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.User)
            .Include(n => n.SenderUser)
            .Include(n => n.Job)
            .Include(n => n.Proposal)
            .Include(n => n.Contract)
            .Include(n => n.Review)
            .Where(n => n.UserId == userId || n.RecipientUserId == userId);

		if (unreadOnly)
		{
			query = query.Where(n => !n.IsRead);
		}

		return await query
			.OrderByDescending(n => n.CreatedOn)
			.ToListAsync();
	}

    public async Task<IEnumerable<Notification>> GetByTemplateIdAsync(int templateId)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.User)
            .Include(n => n.SenderUser)
            .Where(n => n.TemplateId == templateId)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

	public async Task MarkAsReadAsync(int notificationId)
	{
		var notification = await _context.Notifications.FindAsync(notificationId);
		if (notification != null && !notification.IsRead)
		{
			notification.IsRead = true;
			notification.ReadAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();
		}
	}

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && !n.IsRead)
            .ToListAsync();

		foreach (var notification in unreadNotifications)
		{
			notification.IsRead = true;
			notification.ReadAt = DateTime.UtcNow;
		}

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && !n.IsRead)
            .CountAsync();
    }

    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(string userId, int count = 10)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.User)
            .Include(n => n.SenderUser)
            .Where(n => n.UserId == userId || n.RecipientUserId == userId)
            .OrderByDescending(n => n.CreatedOn)
            .Take(count)
            .ToListAsync();
    }

    public async Task DeleteOldNotificationsAsync(string userId, DateTime cutoffDate)
    {
        var oldNotifications = await _context.Notifications
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && n.CreatedOn < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasUnreadNotificationsAsync(string userId)
    {
        return await _context.Notifications
            .AnyAsync(n => (n.UserId == userId || n.RecipientUserId == userId) && !n.IsRead);
    }

    // Enhanced methods for user interaction tracking
    public async Task<IEnumerable<Notification>> GetByRecipientIdAsync(string recipientUserId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Include(n => n.Job)
            .Include(n => n.Proposal)
            .Include(n => n.Contract)
            .Include(n => n.Review)
            .Where(n => n.RecipientUserId == recipientUserId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetBySenderIdAsync(string senderUserId)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Include(n => n.Job)
            .Include(n => n.Proposal)
            .Include(n => n.Contract)
            .Include(n => n.Review)
            .Where(n => n.SenderUserId == senderUserId)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByTypeAsync(string userId, string type, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && n.Type == type);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetInteractionHistoryAsync(string userId1, string userId2)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Where(n => 
                (n.RecipientUserId == userId1 && n.SenderUserId == userId2) ||
                (n.RecipientUserId == userId2 && n.SenderUserId == userId1))
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    // Entity-specific notification methods
    public async Task<IEnumerable<Notification>> GetByJobIdAsync(int jobId)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Include(n => n.Job)
            .Where(n => n.JobId == jobId)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByProposalIdAsync(int proposalId)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Include(n => n.Proposal)
            .Where(n => n.ProposalId == proposalId)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByContractIdAsync(int contractId)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Include(n => n.Contract)
            .Where(n => n.ContractId == contractId)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetByReviewIdAsync(int reviewId)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Include(n => n.Review)
            .Where(n => n.ReviewId == reviewId)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    // Advanced filtering and analytics
    public async Task<IEnumerable<Notification>> GetNotificationsBetweenDatesAsync(string userId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && 
                       n.CreatedOn >= fromDate && n.CreatedOn <= toDate)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUrgentNotificationsAsync(string userId)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && 
                       n.IsUrgent && !n.IsRead)
            .OrderByDescending(n => n.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetExpiringNotificationsAsync(DateTime expiryThreshold)
    {
        return await _context.Notifications
            .Include(n => n.Template)
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Where(n => n.ExpiryDate != null && n.ExpiryDate <= expiryThreshold && n.IsActive)
            .OrderBy(n => n.ExpiryDate)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetNotificationCountsByTypeAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId || n.RecipientUserId == userId)
            .GroupBy(n => n.Type)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    // Bulk operations
    public async Task MarkNotificationsByTypeAsReadAsync(string userId, string type)
    {
        var notifications = await _context.Notifications
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && 
                       n.Type == type && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkNotificationsByJobAsReadAsync(string userId, int jobId)
    {
        var notifications = await _context.Notifications
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && 
                       n.JobId == jobId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkNotificationsByContractAsReadAsync(string userId, int contractId)
    {
        var notifications = await _context.Notifications
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && 
                       n.ContractId == contractId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteNotificationsByTypeAsync(string userId, string type)
    {
        var notifications = await _context.Notifications
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && n.Type == type)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredNotificationsAsync()
    {
        var expiredNotifications = await _context.Notifications
            .Where(n => n.ExpiryDate != null && n.ExpiryDate <= DateTime.UtcNow)
            .ToListAsync();

        _context.Notifications.RemoveRange(expiredNotifications);
        await _context.SaveChangesAsync();
    }

    // Real-time and system methods
    public async Task<bool> HasUnreadNotificationOfTypeAsync(string userId, string type)
    {
        return await _context.Notifications
            .AnyAsync(n => (n.UserId == userId || n.RecipientUserId == userId) && 
                          n.Type == type && !n.IsRead);
    }

    public async Task<DateTime?> GetLastNotificationTimeAsync(string userId, string type)
    {
        var lastNotification = await _context.Notifications
            .Where(n => (n.UserId == userId || n.RecipientUserId == userId) && n.Type == type)
            .OrderByDescending(n => n.CreatedOn)
            .FirstOrDefaultAsync();

        return lastNotification?.CreatedOn;
    }

    public async Task UpdateNotificationAsEmailSentAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsEmailSent = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Notification>> GetPendingEmailNotificationsAsync()
    {
        return await _context.Notifications
            .Include(n => n.RecipientUser)
            .Include(n => n.SenderUser)
            .Where(n => !n.IsEmailSent && n.IsUrgent && n.IsActive)
            .OrderBy(n => n.CreatedOn)
            .ToListAsync();
    }
}