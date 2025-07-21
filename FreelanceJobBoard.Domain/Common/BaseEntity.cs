namespace FreelanceJobBoard.Domain.Common;
public class BaseEntity
{
	public bool IsActive { get; set; }
	public DateTime CreatedOn { get; set; }
	public DateTime? LastUpdatedOn { get; set; }
}
