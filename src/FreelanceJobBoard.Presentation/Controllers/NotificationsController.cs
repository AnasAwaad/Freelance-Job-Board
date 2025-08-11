using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;
public class NotificationsController(NotificationService notificationService) : Controller
{

	[HttpGet]
	public async Task<IActionResult> GetAllNotifications()
	{
		var notifications = await notificationService.GetAllNotificationsAsync();
		return Ok(notifications);
	}
}
