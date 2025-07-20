using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Review : BaseEntity
{
	public int Id { get; set; }
	public int JobId { get; set; }
	public string ReviewerId { get; set; }
	public string RevieweeId { get; set; }
	public int Rating { get; set; }
	public string Comment { get; set; }
	public string ReviewType { get; set; }
	public bool IsVisible { get; set; }

	public Job Job { get; set; }
}

