using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Category : BaseEntity
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	public string? Description { get; set; }

	public ICollection<JobCategory> JobCategories { get; set; }
}