using AutoMapper;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Admin.DTOs;
public class AdminJobProfile : Profile
{
	public AdminJobProfile()
	{
		CreateMap<Job, JobListDto>()
			.ForMember(dest => dest.ClientName,
				opt => opt.MapFrom(src => src.Client != null ? src.Client.User.FullName : null));

		CreateMap<Job, JobDetailsDto>()
			.ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client))
			.ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews));
			
		CreateMap<Client, ClientDto>()
			.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));

		CreateMap<Review, ReviewDto>()
			.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedOn));
	}
}
