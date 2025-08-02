namespace FreelanceJobBoard.Application.Features.Skills.DTOs;

public class CreateSkillDto
{
    public string Name { get; set; } = null!;
}

public class UpdateSkillDto
{
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}