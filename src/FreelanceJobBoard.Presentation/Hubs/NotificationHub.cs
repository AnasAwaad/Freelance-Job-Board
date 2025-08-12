using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Presentation.Hubs;

public class NotificationHub(ApplicationDbContext context, ILogger<NotificationHub> logger) : Hub
{
	public override async Task OnConnectedAsync()
	{
		if (Context.User.IsInRole("Admin"))
			await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

		await base.OnConnectedAsync();
	}

	public override async Task OnDisconnectedAsync(Exception exception)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
		await base.OnDisconnectedAsync(exception);
	}

	public async Task SendNotification(string sender, string title, string message)
	{
		logger.LogInformation("Sending notification: {Message}", message);

		var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "Admin@gmail.com");
		// Create and save notification to DB

		if (admin is not null)
		{
			var notification = new Notification
			{
				Title = title,
				Message = message,
				UserId = admin.Id,
				IsRead = false,
				TemplateId = 1
			};

			context.Notifications.Add(notification);
			await context.SaveChangesAsync();
			await Clients.Group("Admins").SendAsync("ReceiveNotification", sender, title, message);
		}
	}
}
