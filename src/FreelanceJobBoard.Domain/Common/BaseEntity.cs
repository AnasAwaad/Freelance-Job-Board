namespace FreelanceJobBoard.Domain.Common;
public class BaseEntity
{
	public bool IsActive { get; set; } = true;
	public DateTime CreatedOn { get; set; } = DateTime.Now;
	public DateTime? LastUpdatedOn { get; set; }
}
