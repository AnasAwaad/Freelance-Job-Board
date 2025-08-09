using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FreelanceJobBoard.Application.Features.User.Commands.UpdateProfile;

public class UpdateClientProfileCommandHandler : IRequestHandler<UpdateClientProfileCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICloudinaryService _cloudinaryService;

    public UpdateClientProfileCommandHandler(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Unit> Handle(UpdateClientProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unit.Value;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unit.Value;

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;

        if (request.ProfileImageFile != null)
        {
            var url = await _cloudinaryService.UploadFileAsync(request.ProfileImageFile, "profile-images");
            user.ProfileImageUrl = url;
        }

        await _userManager.UpdateAsync(user);

        var client = await _unitOfWork.Clients.GetByUserIdWithDetailsAsync(userId);
        if (client != null)
        {
            if (client.Company == null)
                client.Company = new Domain.Entities.Company();

            client.Company.Name = request.CompanyName ?? client.Company.Name;
            client.Company.Description = request.CompanyDescription ?? client.Company.Description;
            client.Company.LogoUrl = request.CompanyLogoUrl ?? client.Company.LogoUrl;
            client.Company.WebsiteUrl = request.CompanyWebsiteUrl ?? client.Company.WebsiteUrl;
            client.Company.Industry = request.CompanyIndustry ?? client.Company.Industry;
        }

        await _unitOfWork.SaveChangesAsync();
        return Unit.Value;
    }
}
