using AutoMapper;
using FreelanceJobBoard.Application.Features.Notifications.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Notifications.DTOs;

internal class NotificationsProfile : Profile
{
    public NotificationsProfile()
    {
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.Template.TemplateName));
    }
}