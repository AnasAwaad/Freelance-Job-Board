using FreelanceJobBoard.Domain.Common;
using FreelanceJobBoard.Domain.Identity;

namespace FreelanceJobBoard.Domain.Entities;
public class Notification : BaseEntity
{
	public int Id { get; set; }
	public int TemplateId { get; set; }
	public string Title { get; set; } = null!;
	public string Message { get; set; } = null!;
	public bool IsRead { get; set; }
	public DateTime? ReadAt { get; set; }
	public int NotificationTemplateId { get; set; }
	public string UserId { get; set; }
	public ApplicationUser User { get; set; }
	public NotificationTemplate Template { get; set; }
}