using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Commands.UpdateSkill;

internal class UpdateSkillCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateSkillCommand>
{
    public async Task Handle(UpdateSkillCommand request, CancellationToken cancellationToken)
    {
        var skill = await unitOfWork.Skills.GetByIdAsync(request.Id);

        if (skill == null)
        {
            throw new NotFoundException(nameof(Skill), request.Id.ToString());
        }

        var existingSkills = await unitOfWork.Skills.GetAllAsync();
        var skillExists = existingSkills.Any(s => 
            s.Id != request.Id && 
            s.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

        if (skillExists)
        {
            throw new InvalidOperationException($"A skill with the name '{request.Name}' already exists.");
        }

        mapper.Map(request, skill);
        skill.LastUpdatedOn = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();
    }
}