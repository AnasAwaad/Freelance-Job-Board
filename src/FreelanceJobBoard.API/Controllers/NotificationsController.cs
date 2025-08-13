using FreelanceJobBoard.Application.Features.Notifications.DTOs;
using FreelanceJobBoard.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FreelanceJobBoard.API.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

		var notifications = await notificationService.GetUserNotificationsAsync(currentUserService.UserId!, unreadOnly);
		return Ok(notifications);
	}

    [HttpGet("count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        var count = await notificationService.GetUnreadCountAsync(currentUserService.UserId!);
        return Ok(new { count });
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentNotifications([FromQuery] int count = 10)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        var notifications = await notificationService.GetUserNotificationsAsync(currentUserService.UserId!, false);
        var recent = notifications.Take(count);
        return Ok(recent);
    }

    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetNotificationsByType(string type, [FromQuery] bool unreadOnly = false)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        var notifications = await notificationService.GetNotificationsByTypeAsync(currentUserService.UserId!, type);
        var filtered = unreadOnly ? notifications.Where(n => !n.IsRead) : notifications;
        return Ok(filtered);
    }

    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetNotificationsForJob(int jobId)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        var notifications = await notificationService.GetNotificationsForJobAsync(jobId);
        // Filter to only show notifications for the current user
        var userNotifications = notifications.Where(n => n.RecipientUserId == currentUserService.UserId);
        return Ok(userNotifications);
    }

    [HttpGet("contract/{contractId}")]
    public async Task<IActionResult> GetNotificationsForContract(int contractId)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        var notifications = await notificationService.GetNotificationsForContractAsync(contractId);
        // Filter to only show notifications for the current user
        var userNotifications = notifications.Where(n => n.RecipientUserId == currentUserService.UserId);
        return Ok(userNotifications);
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetNotificationAnalytics()
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        var allNotifications = await notificationService.GetUserNotificationsAsync(currentUserService.UserId!, false);
        var unreadCount = await notificationService.GetUnreadCountAsync(currentUserService.UserId!);

        var analytics = new NotificationAnalyticsDto
        {
            TotalNotifications = allNotifications.Count(),
            UnreadNotifications = unreadCount,
            LastNotificationTime = allNotifications.OrderByDescending(n => n.CreatedOn).FirstOrDefault()?.CreatedOn,
            NotificationCountsByType = allNotifications
                .GroupBy(n => n.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            MostActiveNotificationTypes = allNotifications
                .GroupBy(n => n.Type)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList()
        };

        return Ok(analytics);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.MarkAllAsReadAsync(currentUserService.UserId!);
        return NoContent();
    }

    [HttpPut("type/{type}/mark-read")]
    public async Task<IActionResult> MarkAllOfTypeAsRead(string type)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.MarkNotificationsByTypeAsReadAsync(currentUserService.UserId!, type);
        return NoContent();
    }

    [HttpGet("type/{type}/has-unread")]
    public async Task<IActionResult> HasUnreadOfType(string type)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        var hasUnread = await notificationService.HasUnreadNotificationAsync(currentUserService.UserId!, type);
        return Ok(new { hasUnread });
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.CreateNotificationAsync(
            currentUserService.UserId!,
            request.Title,
            request.Message,
            request.TemplateId
        );

        return Created();
    }

    [HttpPost("interaction")]
    public async Task<IActionResult> CreateInteractionNotification([FromBody] CreateInteractionNotificationRequest request)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.CreateInteractionNotificationAsync(
            request.RecipientUserId,
            currentUserService.UserId,
            request.Type,
            request.Title,
            request.Message,
            request.JobId,
            request.ProposalId,
            request.ContractId,
            request.ReviewId,
            request.AdditionalData
        );

        return Created();
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationRequest request)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.SendRealTimeNotificationAsync(
            currentUserService.UserId!,
            request.Title ?? "Test Notification",
            request.Message ?? "This is a test notification",
            new { type = "test", timestamp = DateTime.UtcNow }
        );

        return Ok(new { message = "Test notification sent" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.DeleteNotificationAsync(id);
        return NoContent();
    }

    [HttpDelete("old")]
    public async Task<IActionResult> DeleteOldNotifications([FromQuery] int daysOld = 30)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.DeleteOldNotificationsAsync(
            currentUserService.UserId!,
            TimeSpan.FromDays(daysOld)
        );

        return NoContent();
    }

    // Admin-only endpoints
    [HttpPost("job/{jobId}/approval")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifyJobApproval(int jobId, [FromBody] JobApprovalNotificationRequest request)
    {
        await notificationService.NotifyJobApprovalAsync(jobId, request.IsApproved, request.AdminMessage);
        return NoContent();
    }

    [HttpPost("job/{jobId}/admin-approval-result")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifyClientJobApprovalResult(int jobId, [FromBody] AdminApprovalResultRequest request)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.NotifyClientJobApprovalResultAsync(
            jobId, 
            request.IsApproved, 
            request.AdminMessage, 
            currentUserService.UserId
        );
        return NoContent();
    }

    [HttpPost("job/{jobId}/admin-pending-approval")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> NotifyAdminJobPendingApproval(int jobId)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.NotifyAdminJobPendingApprovalAsync(jobId, currentUserService.UserId!);
        return NoContent();
    }

    [HttpPost("job/{jobId}/admin-review-required")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifyAdminsJobRequiresReview(int jobId, [FromBody] AdminReviewRequiredRequest request)
    {
        await notificationService.NotifyAdminsJobRequiresReviewAsync(jobId, request.Reason);
        return NoContent();
    }

    [HttpPost("job/{jobId}/status-change")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> NotifyJobStatusChange(int jobId, [FromBody] JobStatusChangeNotificationRequest request)
    {
        await notificationService.NotifyJobStatusChangeAsync(jobId, request.Status, request.ClientMessage);
        return NoContent();
    }

    [HttpPost("job/{jobId}/updated")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> NotifyJobUpdated(int jobId)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.NotifyJobUpdatedAsync(jobId, currentUserService.UserId!);
        return NoContent();
    }

    [HttpPost("proposal/{proposalId}/submitted")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> NotifyProposalSubmitted(int proposalId, [FromBody] ProposalSubmittedNotificationRequest request)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.NotifyProposalSubmittedAsync(
            request.JobId, 
            proposalId, 
            currentUserService.UserId!, 
            request.ClientId
        );
        return NoContent();
    }

    [HttpPost("contract/{contractId}/completion-request")]
    public async Task<IActionResult> NotifyContractCompletionRequest(int contractId, [FromBody] ContractCompletionRequestNotificationRequest request)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.NotifyContractCompletionRequestAsync(
            contractId, 
            currentUserService.UserId!, 
            request.TargetUserId, 
            request.JobTitle
        );
        return NoContent();
    }

    [HttpPost("review/{reviewId}/pending")]
    public async Task<IActionResult> NotifyReviewPending(int reviewId, [FromBody] ReviewPendingNotificationRequest request)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.NotifyReviewPendingAsync(
            request.UserId,
            request.CounterpartyName,
            request.JobTitle,
            request.DaysRemaining
        );
        return NoContent();
    }

    [HttpPost("system/maintenance")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifySystemMaintenance([FromBody] SystemMaintenanceNotificationRequest request)
    {
        await notificationService.NotifySystemMaintenanceAsync(request.Message, request.ScheduledTime);
        return NoContent();
    }

    [HttpPost("welcome/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendWelcomeNotification(string userId, [FromBody] WelcomeNotificationRequest request)
    {
        await notificationService.NotifyWelcomeMessageAsync(userId, request.UserName);
        return NoContent();
    }
}

// Request DTOs
public class CreateInteractionNotificationRequest
{
    public string RecipientUserId { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public int? JobId { get; set; }
    public int? ProposalId { get; set; }
    public int? ContractId { get; set; }
    public int? ReviewId { get; set; }
    public object? AdditionalData { get; set; }
}

public class JobApprovalNotificationRequest
{
    public bool IsApproved { get; set; }
    public string? AdminMessage { get; set; }
}

public class AdminApprovalResultRequest
{
    public bool IsApproved { get; set; }
    public string? AdminMessage { get; set; }
}

public class AdminReviewRequiredRequest
{
    public string Reason { get; set; } = "Job requires admin review";
}

public class JobStatusChangeNotificationRequest
{
    public string Status { get; set; } = null!;
    public string? ClientMessage { get; set; }
}

public class ProposalSubmittedNotificationRequest
{
    public int JobId { get; set; }
    public string ClientId { get; set; } = null!;
}

public class ContractCompletionRequestNotificationRequest
{
    public string TargetUserId { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
}

public class ReviewPendingNotificationRequest
{
    public string UserId { get; set; } = null!;
    public string CounterpartyName { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public int DaysRemaining { get; set; }
}

public class NewProposalNotificationRequest
{
    public int JobId { get; set; }
}

public class TestNotificationRequest
{
    public string? Title { get; set; }
    public string? Message { get; set; }
}

public class SystemMaintenanceNotificationRequest
{
    public string Message { get; set; } = null!;
    public DateTime? ScheduledTime { get; set; }
}

public class WelcomeNotificationRequest
{
    public string UserName { get; set; } = null!;
}

public class NotificationAnalyticsDto
{
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public DateTime? LastNotificationTime { get; set; }
    public Dictionary<string, int> NotificationCountsByType { get; set; } = null!;
    public List<string> MostActiveNotificationTypes { get; set; } = null!;
}