using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Commands.DeleteSkill;

internal class DeleteSkillCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteSkillCommand>
{
    public async Task Handle(DeleteSkillCommand request, CancellationToken cancellationToken)
    {
        var skill = await unitOfWork.Skills.GetByIdAsync(request.Id);

        if (skill == null)
        {
            throw new NotFoundException(nameof(Skill), request.Id.ToString());
        }

        var jobSkills = await unitOfWork.JobSkills.GetAllAsync();
        var freelancerSkills = await unitOfWork.FreelancerSkills.GetAllAsync();
        
        var isUsedInJobs = jobSkills.Any(js => js.SkillId == request.Id);
        var isUsedByFreelancers = freelancerSkills.Any(fs => fs.SkillId == request.Id);

        if (isUsedInJobs || isUsedByFreelancers)
        {
            skill.IsActive = false;
            skill.LastUpdatedOn = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();
        }
        else
        {
            unitOfWork.Skills.Delete(skill);
            await unitOfWork.SaveChangesAsync();
        }
    }
}