using AutoMapper;
using FreelanceJobBoard.Application.Features.Auth.DTOs;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;

namespace FreelanceJobBoard.Application.DTOs
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<ApplicationUser, UserInfoDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Will be set manually
                .ForMember(dest => dest.ClientId, opt => opt.Ignore()) // Will be set manually
                .ForMember(dest => dest.FreelancerId, opt => opt.Ignore()); // Will be set manually
        }
    }
}
