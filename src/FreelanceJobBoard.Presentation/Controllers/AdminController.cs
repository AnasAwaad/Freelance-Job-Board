using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Services;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly HttpClient _httpClient;

    public AdminController(ILogger<AdminController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7000/api/");
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

    [HttpGet]
    public async Task<IActionResult> JobManagement(string? status = null)
    {
        try
        {
            var response = await _httpClient.GetAsync($"Admin/jobs?status={status ?? ""}");
            
            if (response.IsSuccessStatusCode)
            {
                var jobs = await response.Content.ReadFromJsonAsync<List<dynamic>>();
                ViewBag.CurrentStatus = status;
                
                // Safely count pending jobs
                ViewBag.PendingCount = 0;
                if (jobs != null)
                {
                    try
                    {
                        ViewBag.PendingCount = jobs.Count(j => 
                        {
                            try
                            {
                                return j.TryGetProperty("status", out System.Text.Json.JsonElement statusProp) && 
                                       statusProp.GetString() == "Pending";
                            }
                            catch
                            {
                                return false;
                            }
                        });
                    }
                    catch (Exception countEx)
                    {
                        _logger.LogWarning(countEx, "Error counting pending jobs");
                        ViewBag.PendingCount = 0;
                    }
                }
                
                return View(jobs ?? new List<dynamic>());
            }
            else
            {
                TempData["Error"] = "Failed to load jobs data.";
                return View(new List<dynamic>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading job management");
            TempData["Error"] = "An error occurred while loading jobs.";
            return View(new List<dynamic>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> ApproveJob(int jobId, string? message = null)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"Admin/jobs/{jobId}/approve", new { message });
            
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Job approved successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to approve job.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving job {JobId}", jobId);
            TempData["Error"] = "An error occurred while approving the job.";
        }

        return RedirectToAction(nameof(JobManagement));
    }

    [HttpPost]
    public async Task<IActionResult> RejectJob(int jobId, string? message = null)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"Admin/jobs/{jobId}/reject", new { message });
            
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Job rejected successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to reject job.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting job {JobId}", jobId);
            TempData["Error"] = "An error occurred while rejecting the job.";
        }

        return RedirectToAction(nameof(JobManagement));
    }

    [HttpGet]
    public async Task<IActionResult> JobDetails(int jobId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"Admin/jobs/{jobId}/details");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var jobDetailsElement = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                
                // Map to strongly-typed view model
                var jobDetailsViewModel = new JobDetailsViewModel();
                
                if (jobDetailsElement.TryGetProperty("id", out var idElement))
                    jobDetailsViewModel.Id = idElement.GetInt32();
                    
                if (jobDetailsElement.TryGetProperty("title", out var titleElement))
                    jobDetailsViewModel.Title = titleElement.GetString() ?? "";
                    
                if (jobDetailsElement.TryGetProperty("description", out var descElement))
                    jobDetailsViewModel.Description = descElement.GetString() ?? "";
                    
                if (jobDetailsElement.TryGetProperty("budgetMin", out var budgetMinElement))
                    jobDetailsViewModel.BudgetMin = budgetMinElement.GetDecimal();
                    
                if (jobDetailsElement.TryGetProperty("budgetMax", out var budgetMaxElement))
                    jobDetailsViewModel.BudgetMax = budgetMaxElement.GetDecimal();
                    
                if (jobDetailsElement.TryGetProperty("status", out var statusElement))
                    jobDetailsViewModel.Status = statusElement.GetString() ?? "";
                    
                if (jobDetailsElement.TryGetProperty("deadline", out var deadlineElement))
                {
                    if (DateTime.TryParse(deadlineElement.GetString(), out var deadline))
                        jobDetailsViewModel.Deadline = deadline;
                }
                
                if (jobDetailsElement.TryGetProperty("createdOn", out var createdOnElement))
                {
                    if (DateTime.TryParse(createdOnElement.GetString(), out var createdOn))
                        jobDetailsViewModel.CreatedOn = createdOn;
                }
                
                // Handle nested client object
                if (jobDetailsElement.TryGetProperty("client", out var clientElement))
                {
                    if (clientElement.TryGetProperty("fullName", out var fullNameElement))
                        jobDetailsViewModel.ClientName = fullNameElement.GetString() ?? "";
                    else if (clientElement.TryGetProperty("company_Name", out var companyNameElement))
                        jobDetailsViewModel.ClientName = companyNameElement.GetString() ?? "";
                    else if (clientElement.TryGetProperty("companyName", out var companyName2Element))
                        jobDetailsViewModel.ClientName = companyName2Element.GetString() ?? "";
                }
                
                return View(jobDetailsViewModel);
            }
            else
            {
                TempData["Error"] = "Failed to load job details.";
                return RedirectToAction(nameof(JobManagement));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading job details for {JobId}", jobId);
            TempData["Error"] = "An error occurred while loading job details.";
            return RedirectToAction(nameof(JobManagement));
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