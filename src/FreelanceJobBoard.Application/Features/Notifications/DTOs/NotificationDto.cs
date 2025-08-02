namespace FreelanceJobBoard.Application.Features.Notifications.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedOn { get; set; }
    public string TemplateName { get; set; } = null!;
}

public class CreateNotificationRequest
{
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public int? TemplateId { get; set; }
}