using AutoMapper;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Reviews.DTOs;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedOn))
            .ForMember(dest => dest.ReviewerName, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.RevieweeName, opt => opt.Ignore()); // Will be set manually

        CreateMap<CreateReviewDto, Review>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastUpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
    }
}