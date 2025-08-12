namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class NotificationViewModel
{
	public int Id { get; set; }
	public string Title { get; set; } = null!;
	public string Message { get; set; } = null!;
	public bool IsRead { get; set; }
	public DateTime? ReadAt { get; set; }
	public DateTime CreatedOn { get; set; }
	public string TemplateName { get; set; } = null!;
}
