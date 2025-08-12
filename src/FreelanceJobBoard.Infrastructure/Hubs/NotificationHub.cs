using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FreelanceJobBoard.Infrastructure.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
        
        _logger.LogWarning("?? [DEBUG] NotificationHub - User attempting connection: UserId={UserId}, UserName={UserName}, ConnectionId={ConnectionId}", 
            userId ?? "NULL", userName, Context.ConnectionId);
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            // Send connection confirmation with user info
            await Clients.Caller.SendAsync("ConnectionEstablished", new { 
                userId = userId,
                userName = userName,
                connectionId = Context.ConnectionId,
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogWarning("?? [DEBUG] NotificationHub - User {UserId} ({UserName}) connected successfully with connection {ConnectionId}, added to group User_{UserId}", 
                userId, userName, Context.ConnectionId, userId);
        }
        else
        {
            _logger.LogError("? [CRITICAL] NotificationHub - User connected without valid authentication! ConnectionId: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            _logger.LogInformation("User {UserId} ({UserName}) disconnected from notification hub", userId, userName);
        }

        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected with error: {ErrorMessage}", userId, exception.Message);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Client can explicitly join their notification group
    public async Task JoinPersonalNotificationGroup()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            await Clients.Caller.SendAsync("JoinedPersonalGroup", userId);
            _logger.LogInformation("User {UserId} explicitly joined their notification group", userId);
        }
    }

    // Client can leave their notification group
    public async Task LeavePersonalNotificationGroup()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            await Clients.Caller.SendAsync("LeftPersonalGroup", userId);
            _logger.LogInformation("User {UserId} left their notification group", userId);
        }
    }

    // Handle notification read acknowledgment from client
    public async Task AcknowledgeNotificationRead(int notificationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Broadcast to all connections for this user that notification was read
            await Clients.Group($"User_{userId}").SendAsync("NotificationReadAcknowledged", notificationId);
            _logger.LogInformation("Notification {NotificationId} read acknowledgment broadcast for user {UserId}", 
                notificationId, userId);
        }
    }

    // Handle marking all notifications as read
    public async Task AcknowledgeAllNotificationsRead()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Group($"User_{userId}").SendAsync("AllNotificationsReadAcknowledged");
            _logger.LogInformation("All notifications read acknowledgment broadcast for user {UserId}", userId);
        }
    }

    // Handle user requesting notification refresh
    public async Task RequestNotificationRefresh()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("RefreshNotificationsRequested");
            _logger.LogInformation("Notification refresh requested by user {UserId}", userId);
        }
    }

    // Handle user presence/activity updates
    public async Task UpdateUserActivity(string activity)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // You could store user activity in cache/database here
            await Clients.Caller.SendAsync("ActivityUpdated", new { 
                activity = activity, 
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogDebug("User {UserId} activity updated: {Activity}", userId, activity);
        }
    }

    // Send test notification (for development/testing)
    public async Task SendTestNotification(string message = "Test notification")
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "User";
        
        if (!string.IsNullOrEmpty(userId))
        {
            var notificationData = new
            {
                title = "Test Notification",
                message = $"Hello {userName}! {message}",
                timestamp = DateTime.UtcNow,
                type = "test",
                data = new { userId = userId, connectionId = Context.ConnectionId }
            };

            await Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", notificationData);
            _logger.LogInformation("Test notification sent to user {UserId}", userId);
        }
    }

    // Get connection info for debugging
    public async Task GetConnectionInfo()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        
        var connectionInfo = new
        {
            connectionId = Context.ConnectionId,
            userId = userId,
            userName = userName,
            userEmail = userEmail,
            timestamp = DateTime.UtcNow,
            userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString(),
            ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString()
        };

        await Clients.Caller.SendAsync("ConnectionInfo", connectionInfo);
        _logger.LogInformation("Connection info sent to user {UserId}", userId);
    }
}