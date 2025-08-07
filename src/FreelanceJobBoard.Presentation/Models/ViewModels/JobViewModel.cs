using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class JobViewModel
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
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
    public ICollection<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    public ICollection<SkillViewModel> Skills { get; set; } = new List<SkillViewModel>();
}