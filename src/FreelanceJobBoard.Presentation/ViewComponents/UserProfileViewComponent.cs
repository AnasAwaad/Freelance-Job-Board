using FreelanceJobBoard.Application.Features.User.DTOs;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.Presentation.ViewComponents;

public class UserProfileViewComponent : ViewComponent
{
    private readonly UserService _userService;
    private readonly ILogger<UserProfileViewComponent> _logger;

    public UserProfileViewComponent(UserService userService, ILogger<UserProfileViewComponent> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new UserProfileViewModel();

        if (User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var profileResponse = await _userService.GetCurrentUserProfileAsync();
                
                if (profileResponse?.UserProfile != null)
                {
                    model.Email = profileResponse.UserProfile.Email;
                    model.FullName = profileResponse.UserProfile.FullName;
                    model.ProfileImageUrl = profileResponse.UserProfile.ProfileImageUrl;
                    model.Role = profileResponse.UserProfile.Roles?.FirstOrDefault();
                }
                else
                {
                    // Fallback to claims
                    SetFallbackData(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile in ViewComponent");
                SetFallbackData(model);
            }
        }

        return View(model);
    }

    private void SetFallbackData(UserProfileViewModel model)
    {
        var claimsPrincipal = User as ClaimsPrincipal;
        model.Email = claimsPrincipal?.FindFirst(ClaimTypes.Email)?.Value;
        model.FullName = claimsPrincipal?.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name;
        model.Role = claimsPrincipal?.FindFirst(ClaimTypes.Role)?.Value;
        model.ProfileImageUrl = null;
    }
}

public class UserProfileViewModel
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Role { get; set; }
    
    public string UserInitial => !string.IsNullOrEmpty(FullName) ? FullName.Substring(0, 1).ToUpper() : "U";
    public bool HasProfilePhoto => !string.IsNullOrEmpty(ProfileImageUrl);
}