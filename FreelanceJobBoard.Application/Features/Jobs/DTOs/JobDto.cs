using FreelanceJobBoard.Application.Features.Categories.DTOs;

namespace FreelanceJobBoard.Application.Features.Jobs.DTOs;
public class JobDto
{
	public int Id { get; set; }
	public int? ClientId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string Status { get; set; }
	public string? RequiredSkills { get; set; }
	public string? Tags { get; set; }
	public int ViewsCount { get; set; }
	public bool IsApproved { get; set; }
	public int? ApprovedBy { get; set; }
	public ICollection<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
	public ICollection<SkillDto> Skills { get; set; } = new List<SkillDto>();
}
