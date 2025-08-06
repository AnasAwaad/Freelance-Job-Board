using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;
public class HomeController : Controller
{
	public IActionResult Index()
	{
		return View();
	}
}
