using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class JobCategory : BaseEntity
{
	public int JobId { get; set; }
	public int CategoryId { get; set; }

	public Job Job { get; set; }
	public Category Category { get; set; }
}