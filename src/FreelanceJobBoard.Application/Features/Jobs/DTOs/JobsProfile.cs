using AutoMapper;
using FreelanceJobBoard.Application.Features.Admin.DTOs;
using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Categories.DTOs;
public class JobsProfile : Profile
{
	public JobsProfile()
	{
		CreateMap<CreateJobCommand, Job>();
		CreateMap<UpdateJobCommand, Job>();


		//CreateMap<Skill, SkillDto>();

		CreateMap<Job, JobDto>()
			.ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
			.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.User.FullName : null))
			.ForMember(dest => dest.ClientProfileImageUrl, opt => opt.MapFrom(src => src.Client != null ? src.Client.User.ProfileImageUrl : null))
			.ForMember(dest => dest.Categories, opt =>
			opt.MapFrom(src => src.Categories.Select(c => new CategoryDto
			{
				Id = c.Category.Id,
				Name = c.Category.Name,
				Description = c.Category.Description
			})))
			.ForMember(dest => dest.Skills, opt =>
			opt.MapFrom(src => src.Skills.Select(s => new SkillDto
			{
				Id = s.Skill.Id,
				Name = s.Skill.Name
			})));

		CreateMap<Job, JobDetailsDto>()
			.ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
			.ForPath(dest => dest.Client.FullName,
			opt => opt.MapFrom(src => src.Client.User.FullName));



		CreateMap<Review, ReviewDto>();

		CreateMap<Job, RecentJobDto>()
			.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedOn))
			.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Split(new[] { ',' }).ToList()))
			.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.User.FullName))
			.ForMember(dest => dest.ClientProfileImage, opt => opt.MapFrom(src => src.Client.User.ProfileImageUrl));


		CreateMap<Job, PublicJobDetailsDto>()
			.ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
			.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Split(new[] { ',' }).ToList()))
			.ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills.Select(s => new SkillDto
			{
				Id = s.Skill.Id,
				Name = s.Skill.Name
			})));

		CreateMap<Job, PublicJobListDto>()
			.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedOn))
			.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.User.FullName))
			.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Split(new[] { ',' }).ToList()))
			.ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills.Select(s => new SkillDto
			{
				Id = s.Skill.Id,
				Name = s.Skill.Name
			})));

		//CreateMap<Job, JobSearchDto>()
		//	.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.User.FullName));



	}
}
