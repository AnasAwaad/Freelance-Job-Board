using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

//[Authorize(Roles = AppRoles.Admin)] // Only admins can manage categories
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

	public async Task<IActionResult> GetTopCategories()
	{
		return Ok(await _categoryService.GetTopCategories(8));
	}

	public async Task<IActionResult> Details(int id)
	{
		var category = await _categoryService.GetCategoryByIdAsync(id);

		if (category == null)
		{
			return NotFound();
		}

		// Convert CategoryFormViewModel to CategoryViewModel for display
		var viewModel = new CategoryViewModel
		{
			Id = category.Id,
			Name = category.Name,
			Description = category.Description,
			IsActive = category.IsActive
		};

		return View(viewModel);
	}

	public IActionResult Create()
	{
		return View(new CreateCategoryViewModel());
	}

	[HttpPost]
	public async Task<IActionResult> Create(CreateCategoryViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		try
		{
			// Convert CreateCategoryViewModel to CategoryFormViewModel
			var categoryForm = new CategoryFormViewModel
			{
				Name = viewModel.Name,
				Description = viewModel.Description,
				IsActive = true // New categories are active by default
			};

			var category = await _categoryService.CreateCategoryAsync(categoryForm);

			if (category != null)
			{
				TempData["Success"] = "Category created successfully!";
				return RedirectToAction(nameof(Details), new { id = category.Id });
			}

			ModelState.AddModelError("", "Failed to create category. The category name might already exist.");
			return View(viewModel);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating category");
			ModelState.AddModelError("", "An error occurred while creating the category.");
			return View(viewModel);
		}
	}

	public async Task<IActionResult> Edit(int id)
	{
		try
		{
			var category = await _categoryService.GetCategoryByIdAsync(id);
			if (category == null)
			{
				return NotFound();
			}

			var viewModel = new UpdateCategoryViewModel
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description,
				IsActive = category.IsActive
			};

			return View(viewModel);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while loading category {Id} for update", id);
			TempData["Error"] = "Error loading category for update";
			return RedirectToAction(nameof(Index));
		}
	}

	[HttpPost]
	public async Task<IActionResult> Edit(UpdateCategoryViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return View(viewModel);
		}

		try
		{
			// Convert UpdateCategoryViewModel to CategoryFormViewModel
			var categoryForm = new CategoryFormViewModel
			{
				Id = viewModel.Id,
				Name = viewModel.Name,
				Description = viewModel.Description,
				IsActive = viewModel.IsActive
			};

			var category = await _categoryService.UpdateCategoryAsync(categoryForm);

			if (category != null)
			{
				TempData["Success"] = "Category updated successfully!";
				return RedirectToAction(nameof(Details), new { id = viewModel.Id });
			}

			ModelState.AddModelError("", "Failed to update category. The category name might already exist.");
			return View(viewModel);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating category {Id}", viewModel.Id);
			ModelState.AddModelError("", "An error occurred while updating the category.");
			return View(viewModel);
		}
	}

	[HttpPost]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var result = await _categoryService.ChangeCategoryStatusAsync(id);
			if (result != null)
			{
				TempData["Success"] = "Category status changed successfully!";
			}
			else
			{
				TempData["Error"] = "Failed to change category status. It might be referenced by existing jobs.";
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while changing status for category {Id}", id);
			TempData["Error"] = "An error occurred while changing category status.";
		}

		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	public async Task<IActionResult> ConfirmDelete(int id)
	{
		var category = await _categoryService.GetCategoryByIdAsync(id);

		if (category == null)
		{
			return NotFound();
		}

		// Convert to CategoryViewModel for the partial view
		var viewModel = new CategoryViewModel
		{
			Id = category.Id,
			Name = category.Name,
			Description = category.Description,
			IsActive = category.IsActive
		};

		return PartialView("_ConfirmDelete", viewModel);
	}

	// AJAX endpoint for creating categories from other forms
	[HttpPost]
	public async Task<IActionResult> CreateAjax([FromBody] CreateCategoryViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		try
		{
			var categoryForm = new CategoryFormViewModel
			{
				Name = viewModel.Name,
				Description = viewModel.Description,
				IsActive = true
			};

			var category = await _categoryService.CreateCategoryAsync(categoryForm);

			if (category != null)
			{
				return Ok(category);
			}

			return BadRequest("Failed to create category");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating category via AJAX");
			return BadRequest("Failed to create category");
		}
	}

	// AJAX endpoint for getting categories for dropdowns
	[HttpGet]
	public async Task<IActionResult> GetCategoriesJson()
	{
		var categories = await _categoryService.GetAllCategoriesAsync();
		return Json(categories);
	}

	// Keep modal-based actions for backward compatibility with existing views
	public IActionResult CreateModal()
	{
		return PartialView("_Form", new CategoryFormViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> CreateModal(CategoryFormViewModel viewModel)
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

	public async Task<IActionResult> UpdateModal(int id)
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
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> UpdateModal(CategoryFormViewModel viewModel)
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
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ChangeStatus(int id)
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
