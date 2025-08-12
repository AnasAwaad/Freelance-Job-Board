using AutoMapper;
using FreelanceJobBoard.Application.Features.Notifications.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Notifications.DTOs;

internal class NotificationsProfile : Profile
{
    public NotificationsProfile()
    {
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Template != null ? src.Template.TemplateName : "general"))
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => "ki-notification-bing"))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => "primary"))
            .ForMember(dest => dest.IsUrgent, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => new Dictionary<string, object>()));
    }
}