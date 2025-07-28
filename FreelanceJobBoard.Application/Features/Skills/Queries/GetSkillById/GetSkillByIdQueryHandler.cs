using AutoMapper;
using FreelanceJobBoard.Application.Features.Skills.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Queries.GetSkillById;

internal class GetSkillByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetSkillByIdQuery, SkillDto>
{
    public async Task<SkillDto> Handle(GetSkillByIdQuery request, CancellationToken cancellationToken)
    {
        var skill = await unitOfWork.Skills.GetByIdAsync(request.Id);

        if (skill == null)
        {
            throw new NotFoundException(nameof(Skill), request.Id.ToString());
        }

        return mapper.Map<SkillDto>(skill);
    }
}