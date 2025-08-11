using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

public class HomeController(HomeService homeService, CategoryService categoryService) : Controller
{
	public async Task<IActionResult> Index()
	{
		var homeViewModel = new HomeViewModel
		{
			TopCategories = await categoryService.GetTopCategories(8),
			RecentJobs = await homeService.GetRecentJobsAsync()

		};

		return View(homeViewModel);
	}

}
