using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class NotificationTemplate : BaseEntity
{
	public int Id { get; set; }
	public string TemplateName { get; set; }
	public string TemplateTitle { get; set; }
	public string TemplateMessage { get; set; }

	public ICollection<Notification> Notifications { get; set; }
}
