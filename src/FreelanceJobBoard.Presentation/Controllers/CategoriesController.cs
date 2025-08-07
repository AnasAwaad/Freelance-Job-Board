using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize] // Require authentication for all actions
public class CategoriesController : Controller
{
	private readonly CategoryService _categoryService;
	private readonly ILogger<CategoriesController> _logger;

	public CategoriesController(CategoryService categoryService, ILogger<CategoriesController> logger)
	{
		_categoryService = categoryService;
		_logger = logger;
	}

	public async Task<IActionResult> Index()
	{
		try
		{
			var categories = await _categoryService.GetAllCategoriesAsync();
			
			// Ensure we always have a non-null collection
			categories ??= new List<CategoryViewModel>();
			
			return View(categories);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while loading categories index");
			TempData["Error"] = "An error occurred while loading categories. Please try again.";
			
			// Return empty list to prevent null reference in view
			return View(new List<CategoryViewModel>());
		}
	}

	//[Authorize(Roles = "Admin")] // Only admins can create categories
	public IActionResult Create()
	{
		return PartialView("_Form", new CategoryFormViewModel());
	}

	[HttpPost]
	[Authorize(Roles = "Admin")] // Only admins can create categories
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CategoryFormViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return PartialView("_Form", viewModel);
		}

		try
		{
			var category = await _categoryService.CreateCategoryAsync(viewModel);
			if (category != null)
			{
				return PartialView("_CategoryRow", category);
			}
			
			ModelState.AddModelError("", "Failed to create category. Please try again.");
			return PartialView("_Form", viewModel);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating category");
			ModelState.AddModelError("", "An error occurred while creating the category.");
			return PartialView("_Form", viewModel);
		}
	}

	[Authorize(Roles = "Admin")] // Only admins can update categories
	public async Task<IActionResult> Update(int id)
	{
		try
		{
			var category = await _categoryService.GetCategoryByIdAsync(id);
			if (category == null)
			{
				return NotFound();
			}
			
			return PartialView("_Form", category);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while loading category {Id} for update", id);
			return BadRequest("Error loading category for update");
		}
	}

	[HttpPost]
	[Authorize(Roles = "Admin")] // Only admins can update categories
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Update(CategoryFormViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return PartialView("_Form", viewModel);
		}

		try
		{
			var category = await _categoryService.UpdateCategoryAsync(viewModel);
			if (category != null)
			{
				return PartialView("_CategoryRow", category);
			}
			
			ModelState.AddModelError("", "Failed to update category. Please try again.");
			return PartialView("_Form", viewModel);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating category {Id}", viewModel.Id);
			ModelState.AddModelError("", "An error occurred while updating the category.");
			return PartialView("_Form", viewModel);
		}
	}

	[HttpPost]
	[Authorize(Roles = "Admin")] // Only admins can change category status
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangeDepartmentStatus(int id)
	{
		try
		{
			var result = await _categoryService.ChangeCategoryStatusAsync(id);
			if (result != null)
			{
				return Ok(result);
			}
			
			return BadRequest("Failed to change category status");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while changing status for category {Id}", id);
			return StatusCode(500, "An error occurred while changing category status");
		}
	}
}
