using FreelanceJobBoard.Domain.Common;
using FreelanceJobBoard.Domain.Identity;

namespace FreelanceJobBoard.Domain.Entities;
public class Freelancer : BaseEntity
{
	public int Id { get; set; }
	public string Bio { get; set; } = null!;
	public string? Description { get; set; }
	public decimal? HourlyRate { get; set; }
	public string? AvailabilityStatus { get; set; }
	public decimal? AverageRating { get; set; }
	public int? TotalReviews { get; set; }

	public string? UserId { get; set; }
	public ApplicationUser? User { get; set; }
	public ICollection<Proposal> Proposals { get; set; }
	public ICollection<FreelancerSkill> FreelancerSkills { get; set; }
	public ICollection<Certification> Certifications { get; set; }
	public ICollection<Contract> Contracts { get; set; }
}