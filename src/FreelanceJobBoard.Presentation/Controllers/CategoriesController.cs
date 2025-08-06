using FreelanceJobBoard.Presentation.Models.ViewModels;
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

	public IActionResult Create()
	{
		return PartialView("_Form");
	}


	[HttpPost]
	public async Task<IActionResult> Create(CategoryFormViewModel viewModel)
	{
		var category = await categoryService.CreateCategoryAsync(viewModel);
		return PartialView("_CategoryRow", category);
	}

	public async Task<IActionResult> Update(int id)
	{
		var category = await categoryService.GetCategoryByIdAsync(id);
		return PartialView("_Form", category);
	}


	[HttpPost]
	public async Task<IActionResult> Update(CategoryFormViewModel viewModel)
	{
		var category = await categoryService.UpdateCategoryAsync(viewModel);
		return PartialView("_CategoryRow", category);
	}

	[HttpPost]
	public async Task<IActionResult> ChangeDepartmentStatus(int id)
	{
		var category = await categoryService.GetCategoryByIdAsync(id);
		if (category is null)
			return NotFound();
		var result = await categoryService.ChangeCategoryStatusAsync(id);
		return Ok(result);
	}
}
