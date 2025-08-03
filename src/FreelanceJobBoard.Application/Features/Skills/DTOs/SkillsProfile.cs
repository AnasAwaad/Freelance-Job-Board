using AutoMapper;
using FreelanceJobBoard.Application.Features.Skills.Commands.CreateSkill;
using FreelanceJobBoard.Application.Features.Skills.Commands.UpdateSkill;
using FreelanceJobBoard.Application.Features.Skills.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Skills.DTOs;

public class SkillsProfile : Profile
{
    public SkillsProfile()
    {
        CreateMap<Skill, SkillDto>();
        CreateMap<CreateSkillCommand, Skill>();
        CreateMap<UpdateSkillCommand, Skill>();
    }
}