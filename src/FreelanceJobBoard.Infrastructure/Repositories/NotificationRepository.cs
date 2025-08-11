using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;

internal class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
	public NotificationRepository(ApplicationDbContext context) : base(context)
	{
	}

	public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false)
	{
		var query = _context.Notifications
			.Include(n => n.Template)
			.Where(n => n.UserId == userId);

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
			.Where(n => n.UserId == userId && !n.IsRead)
			.ToListAsync();

		foreach (var notification in unreadNotifications)
		{
			notification.IsRead = true;
			notification.ReadAt = DateTime.UtcNow;
		}

		await _context.SaveChangesAsync();
	}
}