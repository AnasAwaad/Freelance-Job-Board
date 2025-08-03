using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Commands.CreateSkill;

public class CreateSkillCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateSkillCommand, int>
{
    public async Task<int> Handle(CreateSkillCommand request, CancellationToken cancellationToken)
    {
        var existingSkills = await unitOfWork.Skills.GetAllAsync();
        var skillExists = existingSkills.Any(s => s.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

        if (skillExists)
        {
            throw new InvalidOperationException($"A skill with the name '{request.Name}' already exists.");
        }

        var skill = mapper.Map<Skill>(request);
        skill.IsActive = true;

        await unitOfWork.Skills.CreateAsync(skill);
        await unitOfWork.SaveChangesAsync();

        return skill.Id;
    }
}