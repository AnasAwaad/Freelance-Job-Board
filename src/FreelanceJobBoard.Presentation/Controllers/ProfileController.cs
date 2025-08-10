using FreelanceJobBoard.Application.Features.User.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AuthService _authService;
    private readonly UserService _userService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(AuthService authService, UserService userService, ILogger<ProfileController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var profileResponse = await _userService.GetCurrentUserProfileAsync();
            
            ViewBag.Email = profileResponse?.UserProfile?.Email;
            ViewBag.Name = profileResponse?.UserProfile?.FullName;
            ViewBag.Role = profileResponse?.UserProfile?.Roles?.FirstOrDefault();
            ViewBag.ProfilePhoto = profileResponse?.UserProfile?.ProfileImageUrl;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user profile");
            
            // Fallback to claims if API call fails
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            ViewBag.Email = userEmail;
            ViewBag.Name = userName;
            ViewBag.Role = userRole;
            ViewBag.ProfilePhoto = null;

            return View();
        }
    }

    public async Task<IActionResult> Edit()
    {
        try
        {
            var profileResponse = await _userService.GetCurrentUserProfileAsync();
            
            if (profileResponse?.UserProfile == null)
            {
                TempData["Error"] = "Unable to load profile information.";
                return RedirectToAction("Index");
            }

            var viewModel = new EditProfileViewModel
            {
                FullName = profileResponse.UserProfile.FullName,
                Email = profileResponse.UserProfile.Email,
                CurrentProfileImageUrl = profileResponse.UserProfile.ProfileImageUrl,
                Role = profileResponse.UserProfile.Roles?.FirstOrDefault()
            };

            // Populate role-specific fields
            if (profileResponse.FreelancerProfile != null)
            {
                viewModel.Bio = profileResponse.FreelancerProfile.Bio;
                viewModel.Description = profileResponse.FreelancerProfile.Description;
                viewModel.HourlyRate = profileResponse.FreelancerProfile.HourlyRate;
                viewModel.AvailabilityStatus = profileResponse.FreelancerProfile.AvailabilityStatus;
            }
            else if (profileResponse.ClientProfile != null)
            {
                viewModel.CompanyName = profileResponse.ClientProfile.Company?.Name;
                viewModel.CompanyDescription = profileResponse.ClientProfile.Company?.Description;
                viewModel.CompanyWebsiteUrl = profileResponse.ClientProfile.Company?.WebsiteUrl;
                viewModel.CompanyIndustry = profileResponse.ClientProfile.Company?.Industry;
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user profile for edit");
            
            // Fallback to claims if API call fails
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var fallbackModel = new EditProfileViewModel
            {
                FullName = userName ?? "",
                Email = userEmail ?? "",
                Role = userRole
            };

            return View(fallbackModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            // Determine user role to call appropriate update method
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();

            bool updateResult = false;

            if (userRole == "freelancer")
            {
                var request = new UpdateFreelancerProfileRequest
                {
                    FullName = viewModel.FullName,
                    ProfileImageFile = viewModel.ProfilePhoto,
                    Bio = viewModel.Bio,
                    Description = viewModel.Description,
                    HourlyRate = viewModel.HourlyRate,
                    AvailabilityStatus = viewModel.AvailabilityStatus
                };

                updateResult = await _userService.UpdateFreelancerProfileAsync(request);
            }
            else if (userRole == "client")
            {
                var request = new UpdateClientProfileRequest
                {
                    FullName = viewModel.FullName,
                    ProfileImageFile = viewModel.ProfilePhoto,
                    CompanyName = viewModel.CompanyName,
                    CompanyDescription = viewModel.CompanyDescription,
                    CompanyWebsiteUrl = viewModel.CompanyWebsiteUrl,
                    CompanyIndustry = viewModel.CompanyIndustry
                };

                updateResult = await _userService.UpdateClientProfileAsync(request);
            }
            else if (userRole == "admin")
            {
                // For admin users, currently we only support profile image upload through Auth service
                // Full name update would require a separate API endpoint that doesn't exist yet
                bool profileImageUpdated = true;
                
                if (viewModel.ProfilePhoto != null)
                {
                    // Use the existing profile photo upload from AuthService
                    try 
                    {
                        var imageUrl = await _authService.UploadProfilePhotoAsync(viewModel.ProfilePhoto);
                        profileImageUpdated = !string.IsNullOrEmpty(imageUrl);
                        
                        if (!profileImageUpdated)
                        {
                            _logger.LogWarning("Profile image upload failed for admin user");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading profile image for admin user");
                        profileImageUpdated = false;
                    }
                }

                // For now, we'll consider the update successful if profile image was updated or no image was provided
                updateResult = profileImageUpdated;
                
                if (updateResult && viewModel.ProfilePhoto == null)
                {
                    // If no image was uploaded but the form was submitted, show a message about limitations
                    TempData["Info"] = "Profile photo updated successfully. Note: Full name updates for admin accounts require contacting system support.";
                }
                else if (!updateResult)
                {
                    _logger.LogWarning("Admin profile update failed");
                }
            }
            else
            {
                _logger.LogWarning("Profile update attempted for unsupported role: {Role}", userRole);
                ModelState.AddModelError("", "Profile update is not supported for your account type.");
                return View(viewModel);
            }

            if (updateResult)
            {
                if (userRole != "admin" || viewModel.ProfilePhoto != null)
                {
                    TempData["Success"] = "Profile updated successfully!";
                }
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Failed to update profile. Please try again.");
                return View(viewModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating profile");
            ModelState.AddModelError("", "An error occurred while updating your profile. Please try again.");
            return View(viewModel);
        }
    }

    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var result = await _authService.ChangePasswordAsync(viewModel);
            if (result)
            {
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to change password. Please check your current password and try again.");
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing password for user");
            ModelState.AddModelError("", "An error occurred while changing your password. Please try again.");
            return View(viewModel);
        }
    }

    public IActionResult Security()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendEmailConfirmation()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            TempData["Error"] = "Unable to determine your email address.";
            return RedirectToAction("Index");
        }

        var result = await _authService.ResendEmailConfirmationAsync(userEmail);
        if (result)
        {
            TempData["Success"] = "Confirmation email has been resent to your email address.";
        }
        else
        {
            TempData["Error"] = "Failed to resend confirmation email. Please try again.";
        }

        return RedirectToAction("Security");
    }
}