using FreelanceJobBoard.Application.Features.Skills.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Queries.GetAllSkills;

public class GetAllSkillsQuery : IRequest<IEnumerable<SkillDto>>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}