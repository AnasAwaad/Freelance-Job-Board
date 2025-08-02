using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Commands.UpdateSkill;

public class UpdateSkillCommand : IRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}