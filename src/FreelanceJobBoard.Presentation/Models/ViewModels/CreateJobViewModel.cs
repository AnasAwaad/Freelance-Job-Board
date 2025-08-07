using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class CreateJobViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Minimum budget is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Minimum budget must be greater than 0")]
    public decimal BudgetMin { get; set; }

    [Required(ErrorMessage = "Maximum budget is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Maximum budget must be greater than 0")]
    public decimal BudgetMax { get; set; }

    [Required(ErrorMessage = "Deadline is required")]
    [DataType(DataType.Date)]
    public DateTime Deadline { get; set; }

    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    public string? Tags { get; set; }

    [Required(ErrorMessage = "At least one skill must be selected")]
    public IEnumerable<int> SkillIds { get; set; } = new List<int>();

    [Required(ErrorMessage = "At least one category must be selected")]
    public IEnumerable<int> CategoryIds { get; set; } = new List<int>();

    // For dropdown lists
    public IEnumerable<SkillViewModel> AvailableSkills { get; set; } = new List<SkillViewModel>();
    public IEnumerable<CategoryViewModel> AvailableCategories { get; set; } = new List<CategoryViewModel>();
}