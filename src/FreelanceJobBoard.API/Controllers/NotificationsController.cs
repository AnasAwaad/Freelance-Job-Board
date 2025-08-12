using AutoMapper;
using FreelanceJobBoard.Application.Features.Notifications.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class NotificationsController(INotificationService notificationService,
	ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IMapper mapper) : ControllerBase
{
	[HttpGet("user")]
	public async Task<IActionResult> GetUserNotifications()
	{
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


		var notifications = await unitOfWork.Notifications
			.GetByUserIdAsync(userId);



		return Ok(mapper.Map<IEnumerable<NotificationDto>>(notifications));
	}

	[HttpPost]
	public async Task<IActionResult> SendNotification(string userId, string template, string title, string message)
	{
		// 1. Save to DB
		var notification = new Notification
		{
			UserId = userId,
			Title = title,
			Message = message,
			IsRead = false,
			CreatedOn = DateTime.UtcNow,
			NotificationTemplateId = 1,

		};
		await unitOfWork.Notifications.CreateAsync(notification);
		await unitOfWork.SaveChangesAsync();

		//await hubContext.Clients.User(userId).SendAsync("ReceiveNotification", title, message);
		return Ok(message);
	}

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