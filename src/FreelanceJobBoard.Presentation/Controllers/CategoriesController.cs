using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;
public class CategoriesController(CategoryService categoryService) : Controller
{
	[Authorize(Roles = "Client")]
	public async Task<IActionResult> Index()
	{
		var categories = await categoryService.GetAllCategoriesAsync();
		return View(categories);
	}
}
