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
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(AuthService authService, ILogger<ProfileController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        ViewBag.Email = userEmail;
        ViewBag.Name = userName;
        ViewBag.Role = userRole;

        return View();
    }

    public IActionResult Edit()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;

        ViewBag.Email = userEmail;
        ViewBag.Name = userName;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string fullName, string email)
    {
        // TODO: Implement profile update logic with proper service call
        if (string.IsNullOrWhiteSpace(fullName))
        {
            ModelState.AddModelError("fullName", "Full name is required.");
            ViewBag.Name = fullName;
            ViewBag.Email = email;
            return View();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("email", "Email is required.");
            ViewBag.Name = fullName;
            ViewBag.Email = email;
            return View();
        }

        // For now, just show success message
        TempData["Success"] = "Profile updated successfully!";
        return RedirectToAction("Index");
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