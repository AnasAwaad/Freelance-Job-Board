using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class SkillViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}