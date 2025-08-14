using AutoMapper;
using FreelanceJobBoard.Application.Features.Admin.DTOs;
using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Features.User.DTOs;
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
			.ForMember(dest => dest.ClientAverageRating, opt => opt.MapFrom(src => src.Client != null ? src.Client.AverageRating : 0))
			.ForMember(dest => dest.ClientTotalReviews, opt => opt.MapFrom(src => src.Client != null ? src.Client.TotalReviews : 0))
			.ForMember(dest => dest.AssignedFreelancerName, opt => opt.MapFrom(src => 
				src.Proposals != null && src.Proposals.Any(p => p.Status == "Accepted") 
					? src.Proposals.FirstOrDefault(p => p.Status == "Accepted")!.Freelancer.User.FullName 
					: null))
			.ForMember(dest => dest.AssignedFreelancerProfileImageUrl, opt => opt.MapFrom(src => 
				src.Proposals != null && src.Proposals.Any(p => p.Status == "Accepted") 
					? src.Proposals.FirstOrDefault(p => p.Status == "Accepted")!.Freelancer.User.ProfileImageUrl 
					: null))
			.ForMember(dest => dest.AssignedFreelancerAverageRating, opt => opt.MapFrom(src => 
				src.Proposals != null && src.Proposals.Any(p => p.Status == "Accepted") 
					? src.Proposals.FirstOrDefault(p => p.Status == "Accepted")!.Freelancer.AverageRating 
					: 0))
			.ForMember(dest => dest.AssignedFreelancerTotalReviews, opt => opt.MapFrom(src => 
				src.Proposals != null && src.Proposals.Any(p => p.Status == "Accepted") 
					? src.Proposals.FirstOrDefault(p => p.Status == "Accepted")!.Freelancer.TotalReviews 
					: 0))
			.ForMember(dest => dest.Categories, opt =>
			opt.MapFrom(src => src.Categories.Select(c => new CategoryDto
			{
				Id = c.Category.Id,
				Name = c.Category.Name,
				Description = c.Category.Description
			})))
			.ForMember(dest => dest.Skills, opt =>
			opt.MapFrom(src => src.Skills.Select(s => new FreelanceJobBoard.Application.Features.Jobs.DTOs.SkillDto
			{
				Id = s.Skill.Id,
				Name = s.Skill.Name
			})));

		CreateMap<Job, JobDetailsDto>()
			.ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
			.ForPath(dest => dest.Client.Id, opt => opt.MapFrom(src => src.Client.Id))
			.ForPath(dest => dest.Client.FullName, opt => opt.MapFrom(src => src.Client.User.FullName))
			.ForPath(dest => dest.Client.AverageRating, opt => opt.MapFrom(src => src.Client.AverageRating))
			.ForPath(dest => dest.Client.TotalReviews, opt => opt.MapFrom(src => src.Client.TotalReviews))
			.ForPath(dest => dest.Client.ProfileImageUrl, opt => opt.MapFrom(src => src.Client.User.ProfileImageUrl));



		CreateMap<Review, ReviewDto>();

		CreateMap<Job, RecentJobDto>()
			.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedOn))
			.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Split(new[] { ',' }).ToList()))
			.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.User.FullName))
			.ForMember(dest => dest.ClientProfileImage, opt => opt.MapFrom(src => src.Client.User.ProfileImageUrl));


		CreateMap<Job, PublicJobDetailsDto>()
			.ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn))
			.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Split(new[] { ',' }).ToList()))
			.ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills.Select(s => new FreelanceJobBoard.Application.Features.Jobs.DTOs.SkillDto
			{
				Id = s.Skill.Id,
				Name = s.Skill.Name
			})))
			.ForMember(dest => dest.Client, opt => opt.MapFrom(src => new PublicClientDto
			{
				FullName = src.Client.User.FullName,
				ProfileImageUrl = src.Client.User.ProfileImageUrl,
				AverageRating = src.Client.AverageRating,
				TotalReviews = src.Client.TotalReviews,
				Company = new CompanyDto
				{
					Name = src.Client.Company.Name,
					Description = src.Client.Company.Description,
					LogoUrl = src.Client.Company.LogoUrl,
					WebsiteUrl = src.Client.Company.WebsiteUrl,
					Industry = src.Client.Company.Industry
				}
			}));

		CreateMap<Job, PublicJobListDto>()
			.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedOn))
			.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.User.FullName))
			.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Split(new[] { ',' }).ToList()))
			.ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.Skills.Select(s => new FreelanceJobBoard.Application.Features.Jobs.DTOs.SkillDto
			{
				Id = s.Skill.Id,
				Name = s.Skill.Name
			})));

		//CreateMap<Job, JobSearchDto>()
		//	.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.User.FullName));



	}
}
