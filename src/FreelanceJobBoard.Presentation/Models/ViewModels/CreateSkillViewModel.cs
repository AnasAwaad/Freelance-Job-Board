using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class CreateSkillViewModel
{
    [Required(ErrorMessage = "Skill name is required")]
    [StringLength(255, ErrorMessage = "Skill name cannot exceed 255 characters")]
    [Display(Name = "Skill Name")]
    public string Name { get; set; } = string.Empty;
}