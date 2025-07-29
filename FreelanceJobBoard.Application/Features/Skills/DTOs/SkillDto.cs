namespace FreelanceJobBoard.Application.Features.Skills.DTOs;

public class SkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}