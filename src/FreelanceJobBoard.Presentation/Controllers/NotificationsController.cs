using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Presentation.Services;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FreelanceJobBoard.Application.Features.Notifications.DTOs;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    // Display all notifications page
    public async Task<IActionResult> Index([FromQuery] bool unreadOnly = false)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            ViewBag.UnreadOnly = unreadOnly;
            ViewBag.UnreadCount = unreadCount;
            ViewBag.TotalCount = notifications.Count();

            return View(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notifications page");
            TempData["Error"] = "Failed to load notifications. Please try again.";
            return RedirectToAction("Index", "Home");
        }
    }

    // API endpoint for AJAX calls
	[HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new
            {
                success = true,
                notifications = notifications.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    isRead = n.IsRead,
                    createdOn = n.CreatedOn,
                    readAt = n.ReadAt,
                    type = n.Type
                }),
                unreadCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return Json(new { success = false, message = "Failed to load notifications" });
        }
    }

    // Dashboard notifications (limited to recent 5)
    [HttpGet]
    public async Task<IActionResult> GetDashboardNotifications()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var allNotifications = await _notificationService.GetUserNotificationsAsync(userId, false);
            var allNotificationsList = allNotifications.ToList();
            
            var recentNotifications = allNotificationsList.Take(5).ToList();
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new
            {
                success = true,
                notifications = recentNotifications.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    isRead = n.IsRead,
                    createdOn = n.CreatedOn,
                    readAt = n.ReadAt,
                    type = n.Type
                }),
                totalCount = allNotificationsList.Count,
                unreadCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard notifications");
            return Json(new { success = false, message = "Failed to load notifications" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, count = 0 });
            }

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { success = true, count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return Json(new { success = false, count = 0 });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _notificationService.MarkAsReadAsync(id);
            var newUnreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new { success = true, unreadCount = newUnreadCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return Json(new { success = false, message = "Failed to mark notification as read" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _notificationService.MarkAllAsReadAsync(userId);

            return Json(new { success = true, unreadCount = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return Json(new { success = false, message = "Failed to mark all notifications as read" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _notificationService.DeleteNotificationAsync(id);
            var newUnreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new { success = true, unreadCount = newUnreadCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            return Json(new { success = false, message = "Failed to delete notification" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            // Delete all notifications for the user (older than 0 days = all)
            await _notificationService.DeleteOldNotificationsAsync(userId, TimeSpan.Zero);

            return Json(new { success = true, unreadCount = 0, message = "All notifications deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all notifications");
            return Json(new { success = false, message = "Failed to delete all notifications" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteOld(int daysOld = 30)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            await _notificationService.DeleteOldNotificationsAsync(userId, TimeSpan.FromDays(daysOld));
            var newUnreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new { success = true, unreadCount = newUnreadCount, message = $"Notifications older than {daysOld} days deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old notifications");
            return Json(new { success = false, message = "Failed to delete old notifications" });
        }
    }

    // Notification Settings
    public IActionResult Settings()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // For now, return default settings. In a full implementation, 
            // you would load user preferences from database
            var model = new NotificationSettingsViewModel
            {
                UserId = userId,
                UserEmail = userEmail ?? "",
                FullName = userName ?? ""
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notification settings");
            TempData["Error"] = "Failed to load notification settings. Please try again.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(NotificationSettingsViewModel model)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return View("Settings", model);
            }

            // TODO: Implement actual saving of notification settings to database
            // This would typically involve a UserNotificationSettings table
            _logger.LogInformation("Notification settings updated for user {UserId}", userId);

            TempData["Success"] = "Notification settings updated successfully!";
            return RedirectToAction("Settings");
        }
        catch (Exception ex)
	{
            _logger.LogError(ex, "Error updating notification settings");
            TempData["Error"] = "Failed to update notification settings. Please try again.";
            return View("Settings", model);
        }
	}
}
