using FreelanceJobBoard.Application.Features.Skills.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Skills.Queries.GetSkillById;

public class GetSkillByIdQuery : IRequest<SkillDto>
{
    public int Id { get; set; }

    public GetSkillByIdQuery(int id)
    {
        Id = id;
    }
}