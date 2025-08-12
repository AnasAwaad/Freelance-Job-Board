using FreelanceJobBoard.Application.Features.Notifications.DTOs;
using FreelanceJobBoard.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;

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

        var notifications = await notificationService.GetUserNotificationsAsync(currentUserService.UserId!, unreadOnly: true);
        return Ok(new { count = notifications.Count() });
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

        var notifications = await notificationService.GetUserNotificationsAsync(currentUserService.UserId!, unreadOnly: true);
        
        foreach (var notification in notifications)
        {
            await notificationService.MarkAsReadAsync(notification.Id);
        }

        return NoContent();
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

    [HttpPost("job/{jobId}/approval")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NotifyJobApproval(int jobId, [FromBody] JobApprovalNotificationRequest request)
    {
        await notificationService.NotifyJobApprovalAsync(jobId, request.IsApproved, request.AdminMessage);
        return NoContent();
    }

    [HttpPost("job/{jobId}/status-change")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> NotifyJobStatusChange(int jobId, [FromBody] JobStatusChangeNotificationRequest request)
    {
        await notificationService.NotifyJobStatusChangeAsync(jobId, request.Status, request.ClientMessage);
        return NoContent();
    }

    [HttpPost("proposal/{proposalId}/new")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> NotifyNewProposal(int proposalId, [FromBody] NewProposalNotificationRequest request)
    {
        await notificationService.NotifyNewProposalAsync(request.JobId, proposalId);
        return NoContent();
    }
}

public class JobApprovalNotificationRequest
{
    public bool IsApproved { get; set; }
    public string? AdminMessage { get; set; }
}

public class JobStatusChangeNotificationRequest
{
    public string Status { get; set; } = null!;
    public string? ClientMessage { get; set; }
}

public class NewProposalNotificationRequest
{
    public int JobId { get; set; }
}