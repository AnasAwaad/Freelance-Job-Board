using AutoMapper;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.User.DTOs;
public class ClientProfile : Profile
{
	public ClientProfile()
	{
		CreateMap<Client, PublicClientDto>()
			.ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl))
			.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName));
		CreateMap<Company, CompanyDto>();

	}
}
