using FreelanceJobBoard.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;

    public AdminController(ILogger<AdminController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        try
        {
            // Log admin access for security auditing
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation("Admin panel accessed by user: {Email}, Role: {Role}", userEmail, userRole);

            // Verify user has admin role
            if (!User.IsInRole(AppRoles.Admin))
            {
                _logger.LogWarning("Unauthorized access attempt to admin panel by user: {Email}", userEmail);
                return RedirectToAction("AccessDenied", "Auth");
            }

            ViewBag.UserEmail = userEmail;
            ViewBag.UserRole = userRole;
            ViewBag.IsAdmin = User.IsInRole(AppRoles.Admin);

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while accessing admin panel");
            TempData["Error"] = "An error occurred while loading the admin panel.";
            return RedirectToAction("Index", "Home");
        }
    }

    [AllowAnonymous] // Temporarily allow anonymous access for debugging
    public IActionResult Diagnostics()
    {
        var diagnosticInfo = new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name,
            AuthenticationType = User.Identity?.AuthenticationType,
            Claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList(),
            IsInAdminRole = User.IsInRole(AppRoles.Admin),
            Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
            AppRolesAdmin = AppRoles.Admin,
            Timestamp = DateTime.Now
        };

        ViewBag.DiagnosticInfo = diagnosticInfo;
        return View(diagnosticInfo);
    }

    [HttpGet]
    public IActionResult UserManagement()
    {
        if (!User.IsInRole(AppRoles.Admin))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        return View();
    }

    [HttpGet]
    public IActionResult SystemSettings()
    {
        if (!User.IsInRole(AppRoles.Admin))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        return View();
    }

    [HttpGet]
    public IActionResult Reports()
    {
        if (!User.IsInRole(AppRoles.Admin))
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        return View();
    }
}