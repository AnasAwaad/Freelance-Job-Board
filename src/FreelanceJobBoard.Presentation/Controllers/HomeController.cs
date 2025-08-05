using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // You can later call services to pass data to the view
        return View();
    }
}

