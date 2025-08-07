using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class UpdateSkillViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Skill name is required")]
    [StringLength(255, ErrorMessage = "Skill name cannot exceed 255 characters")]
    [Display(Name = "Skill Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}