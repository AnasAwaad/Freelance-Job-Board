using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize]
public class HomeController(HomeService homeService) : Controller
{
	public async Task<IActionResult> Index()
	{
		var homeViewModel = new HomeViewModel
		{
			RecentJobs = await homeService.GetRecentJobsAsync()
		};

		return View(homeViewModel);
	}
}
