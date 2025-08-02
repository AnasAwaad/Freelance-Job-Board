using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Commands.CreateSkill;

public class CreateSkillCommand : IRequest<int>
{
    public string Name { get; set; } = null!;
}