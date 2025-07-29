using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false);
    Task<IEnumerable<Notification>> GetByTemplateIdAsync(int templateId);
    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
}