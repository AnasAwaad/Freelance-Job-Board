using FreelanceJobBoard.Domain.Common;
using FreelanceJobBoard.Domain.Identity;

namespace FreelanceJobBoard.Domain.Entities;
public class JobView : BaseEntity
{
	public int Id { get; set; }
	public int JobId { get; set; }
	public DateTime ViewedAt { get; set; }
	public string? IpAddress { get; set; }

	public string UserId { get; set; }
	public Job Job { get; set; }
	public ApplicationUser User { get; set; }
}
