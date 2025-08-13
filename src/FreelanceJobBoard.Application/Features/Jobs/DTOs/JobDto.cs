using FreelanceJobBoard.Application.Features.Categories.DTOs;

namespace FreelanceJobBoard.Application.Features.Jobs.DTOs;
public class JobDto
{
	public int Id { get; set; }
	public int? ClientId { get; set; }
	public string? ClientName { get; set; }
	public string? ClientProfileImageUrl { get; set; }
	public decimal ClientAverageRating { get; set; }
	public int ClientTotalReviews { get; set; }
	public string? AssignedFreelancerName { get; set; }
	public string? AssignedFreelancerProfileImageUrl { get; set; }
	public decimal AssignedFreelancerAverageRating { get; set; }
	public int AssignedFreelancerTotalReviews { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string Status { get; set; } = null!;
	public string? RequiredSkills { get; set; }
	public string? Tags { get; set; }
	public int ViewsCount { get; set; }
	public bool IsApproved { get; set; }
	public int? ApprovedBy { get; set; }
	public DateTime CreatedOn { get; set; }
	public ICollection<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
	public ICollection<SkillDto> Skills { get; set; } = new List<SkillDto>();
}
