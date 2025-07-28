using AutoMapper;
using FreelanceJobBoard.Application.Features.Skills.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Queries.GetAllSkills;

internal class GetAllSkillsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetAllSkillsQuery, IEnumerable<SkillDto>>
{
    public async Task<IEnumerable<SkillDto>> Handle(GetAllSkillsQuery request, CancellationToken cancellationToken)
    {
        var skills = await unitOfWork.Skills.GetAllAsync();

        if (!string.IsNullOrEmpty(request.Search))
        {
            var searchTerm = request.Search.ToLower().Trim();
            skills = skills.Where(s => s.Name.ToLower().Contains(searchTerm));
        }

        if (request.IsActive.HasValue)
        {
            skills = skills.Where(s => s.IsActive == request.IsActive.Value);
        }

        skills = skills.OrderBy(s => s.Name);

        return mapper.Map<IEnumerable<SkillDto>>(skills);
    }
}