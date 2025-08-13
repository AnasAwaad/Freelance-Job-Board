using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.Presentation.Controllers;

public class HomeController : Controller
{
	private readonly IDashboardService _dashboardService;
	private readonly ILogger<HomeController> _logger;
	private readonly HomeService _homeService;
	private readonly CategoryService _categoryService;

	public HomeController(
		IDashboardService dashboardService,
		ILogger<HomeController> logger,
		HomeService homeService,
		CategoryService categoryService)
	{
		_dashboardService = dashboardService;
		_logger = logger;
		_homeService = homeService;
		_categoryService = categoryService;
	}

	public async Task<IActionResult> Index()
	{
		var homeViewModel = new HomeViewModel
		{
			TopCategories = await _categoryService.GetTopCategories(8),
			RecentJobs = await _homeService.GetRecentJobsAsync()

		};

		return View(homeViewModel);
	}

	[HttpGet]
	public async Task<IActionResult> GetDashboardStats()
	{
		try
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Json(new { success = false, message = "Unauthorized" });
			}

			var stats = await _dashboardService.GetDashboardStatsAsync(userId);

			return Json(new
			{
				success = true,
				activeJobs = stats.ActiveJobs,
				proposals = stats.Proposals,
				contracts = stats.Contracts,
				reviews = stats.Reviews,
				pendingApprovals = stats.PendingApprovals,
				userRole = stats.UserRole
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading dashboard stats");
			return Json(new { success = false, message = "Failed to load dashboard statistics" });
		}
	}

	[HttpGet]
	public async Task<IActionResult> GetRecentActivity()
	{
		try
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Json(new { success = false, message = "Unauthorized" });
			}

			var activity = await _dashboardService.GetRecentActivityAsync(userId, 10);

			return Json(new
			{
				success = true,
				activities = activity.Activities.Select(a => new
				{
					id = a.Id,
					title = a.Title,
					description = a.Description,
					timestamp = a.Timestamp,
					type = a.Type,
					icon = a.Icon,
					color = a.Color,
					actionUrl = a.ActionUrl,
					timeAgo = GetTimeAgo(a.Timestamp)
				})
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading recent activity");
			return Json(new { success = false, message = "Failed to load recent activity" });
		}
	}

	[HttpGet]
	public async Task<IActionResult> GetDetailedAnalytics()
	{
		try
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Json(new { success = false, message = "Unauthorized" });
			}

			var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
			var primaryRole = userRoles.FirstOrDefault() ?? "User";

			object detailedStats = primaryRole switch
			{
				"Client" => await _dashboardService.GetClientDashboardAsync(userId),
				"Freelancer" => await _dashboardService.GetFreelancerDashboardAsync(userId),
				"Admin" => await _dashboardService.GetAdminDashboardAsync(),
				_ => await _dashboardService.GetDashboardStatsAsync(userId)
			};

			return Json(new
			{
				success = true,
				data = detailedStats,
				userRole = primaryRole
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading detailed analytics");
			return Json(new { success = false, message = "Failed to load detailed analytics" });
		}
	}

	[HttpGet]
	public async Task<IActionResult> GetEarningsAnalytics()
	{
		try
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Json(new { success = false, message = "Unauthorized" });
			}

			var earnings = await _dashboardService.GetEarningsAnalyticsAsync(userId);

			return Json(new
			{
				success = true,
				data = earnings
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading earnings analytics");
			return Json(new { success = false, message = "Failed to load earnings analytics" });
		}
	}

	[HttpGet]
	public async Task<IActionResult> GetSpendingAnalytics()
	{
		try
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Json(new { success = false, message = "Unauthorized" });
			}

			var spending = await _dashboardService.GetSpendingAnalyticsAsync(userId);

			return Json(new
			{
				success = true,
				data = spending
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading spending analytics");
			return Json(new { success = false, message = "Failed to load spending analytics" });
		}
	}

	[HttpGet]
	public async Task<IActionResult> GetJobApplicationFunnel()
	{
		try
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Json(new { success = false, message = "Unauthorized" });
			}

			var funnel = await _dashboardService.GetJobApplicationFunnelAsync(userId);

			return Json(new
			{
				success = true,
				data = funnel
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading job application funnel");
			return Json(new { success = false, message = "Failed to load job application funnel" });
		}
	}

	[HttpGet]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> GetTopUsers(string type = "freelancers", int limit = 10)
	{
		try
		{
			var topUsers = type.ToLower() switch
			{
				"clients" => await _dashboardService.GetTopClientsAsync(limit),
				_ => await _dashboardService.GetTopFreelancersAsync(limit)
			};

			return Json(new
			{
				success = true,
				data = topUsers
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading top users");
			return Json(new { success = false, message = "Failed to load top users" });
		}
	}

	private static string GetTimeAgo(DateTime dateTime)
	{
		var timeSpan = DateTime.UtcNow - dateTime;

		if (timeSpan.TotalDays >= 365)
		{
			var years = (int)(timeSpan.TotalDays / 365);
			return $"{years} year{(years == 1 ? "" : "s")} ago";
		}

		if (timeSpan.TotalDays >= 30)
		{
			var months = (int)(timeSpan.TotalDays / 30);
			return $"{months} month{(months == 1 ? "" : "s")} ago";
		}

		if (timeSpan.TotalDays >= 1)
		{
			var days = (int)timeSpan.TotalDays;
			return $"{days} day{(days == 1 ? "" : "s")} ago";
		}

		if (timeSpan.TotalHours >= 1)
		{
			var hours = (int)timeSpan.TotalHours;
			return $"{hours} hour{(hours == 1 ? "" : "s")} ago";
		}

		if (timeSpan.TotalMinutes >= 1)
		{
			var minutes = (int)timeSpan.TotalMinutes;
			return $"{minutes} minute{(minutes == 1 ? "" : "s")} ago";
		}

		return "Just now";
	}


}
