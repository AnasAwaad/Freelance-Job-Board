using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FreelanceJobBoard.Application.Features.User.Commands.UpdateProfile;

public class UpdateFreelancerProfileCommandHandler : IRequestHandler<UpdateFreelancerProfileCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICloudinaryService _cloudinaryService;

    public UpdateFreelancerProfileCommandHandler(
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

    public async Task<Unit> Handle(UpdateFreelancerProfileCommand request, CancellationToken cancellationToken)
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

        var freelancer = await _unitOfWork.Freelancers.GetByUserIdAsync(userId);
        if (freelancer != null)
        {
            freelancer.Bio = request.Bio ?? freelancer.Bio;
            freelancer.Description = request.Description;
            freelancer.HourlyRate = request.HourlyRate;
            freelancer.AvailabilityStatus = request.AvailabilityStatus;
        }

        await _unitOfWork.SaveChangesAsync();
        return Unit.Value;
    }
}
