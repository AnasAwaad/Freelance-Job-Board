using FreelanceJobBoard.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize(Roles = AppRoles.Client)]
public class ClientsController : Controller
{
	public IActionResult Index()
	{
		return View();
	}
}
