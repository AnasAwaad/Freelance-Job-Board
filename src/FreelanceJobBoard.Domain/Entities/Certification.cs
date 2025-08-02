using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Certification : BaseEntity
{
	public int Id { get; set; }
	public int FreelancerId { get; set; }
	public string Name { get; set; } = null!;
	public string? Provider { get; set; }
	public string? Description { get; set; }
	public DateTime DateEarned { get; set; }
	public string? CertificationLink { get; set; }

	public Freelancer Freelancer { get; set; }
}
