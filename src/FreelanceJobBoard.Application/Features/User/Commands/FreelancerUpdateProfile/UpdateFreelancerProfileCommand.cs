using MediatR;
using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Application.Features.User.Commands.UpdateProfile;

public class UpdateFreelancerProfileCommand : IRequest<Unit>
{
    public string? FullName { get; set; }
    public IFormFile? ProfileImageFile { get; set; }

    public string? Bio { get; set; }
    public string? Description { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? AvailabilityStatus { get; set; }
}