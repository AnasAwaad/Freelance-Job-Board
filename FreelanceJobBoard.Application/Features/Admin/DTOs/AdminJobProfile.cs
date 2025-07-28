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
	}
}
