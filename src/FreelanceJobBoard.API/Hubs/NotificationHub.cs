using Microsoft.AspNetCore.SignalR;

namespace FreelanceJobBoard.API.Hubs;

public class NotificationHub : Hub
{
	private readonly ILogger<NotificationHub> logger;

	public NotificationHub(ILogger<NotificationHub> logger)
	{
		this.logger = logger;
	}

	public async Task SendNotification(string userId, string title, string message)
	{
		logger.LogInformation("Sending notification: {Message}", message);
		await Clients.User(userId).SendAsync("ReceiveNotification", title, message);
	}
}
