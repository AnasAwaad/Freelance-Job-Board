using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;
public class ClientsController : Controller
{
	public IActionResult Index()
	{
		return View();
	}
}
