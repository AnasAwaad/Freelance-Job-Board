using FreelanceJobBoard.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;

[Route("api/[controller]")]
[ApiController]
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


    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (!currentUserService.IsAuthenticated)
            return Unauthorized();

        await notificationService.MarkAsReadAsync(id);
        return NoContent();
    }


    [HttpPost("job/{jobId}/approval")]
    public async Task<IActionResult> NotifyJobApproval(int jobId, [FromBody] JobApprovalNotificationRequest request)
    {
        // TODO: Add admin authorization check here
        await notificationService.NotifyJobApprovalAsync(jobId, request.IsApproved, request.AdminMessage);
        return NoContent();
    }
}

public class JobApprovalNotificationRequest
{
    public bool IsApproved { get; set; }
    public string? AdminMessage { get; set; }
}