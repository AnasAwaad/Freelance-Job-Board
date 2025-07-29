using MediatR;
using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Application.Features.User.Commands.UpdateProfile;

public class UpdateClientProfileCommand : IRequest<Unit>
{
    public string? FullName { get; set; }
    public IFormFile? ProfileImageFile { get; set; }

    public string? CompanyName { get; set; }
    public string? CompanyDescription { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string? CompanyWebsiteUrl { get; set; }
    public string? CompanyIndustry { get; set; }
}
