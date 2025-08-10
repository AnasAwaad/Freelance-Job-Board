using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

//[Authorize] 
public class JobsController : Controller
{
	private readonly JobService _jobService;
	private readonly CategoryService _categoryService;
	private readonly SkillService _skillService;

	public JobsController(JobService jobService, CategoryService categoryService, SkillService skillService)
	{
		_jobService = jobService;
		_categoryService = categoryService;
		_skillService = skillService;
	}

	public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = null)
	{
		var jobs = await _jobService.GetAllJobsAsync(pageNumber, pageSize, search, sortBy, sortDirection);

		ViewBag.Search = search;
		ViewBag.SortBy = sortBy;
		ViewBag.SortDirection = sortDirection;

		return View(jobs);
	}

	public async Task<IActionResult> MyJobs()
	{
		var jobs = await _jobService.GetMyJobsAsync();
		return View(jobs);
	}
	[AllowAnonymous]

	public IActionResult PublicJobDetails(int jobId)
	{
		var jobDetails = _jobService.GetPublicJobDeatils(jobId).Result;
		if (jobDetails == null)
		{
			return NotFound();
		}
		return View(jobDetails);
	}

	public async Task<IActionResult> Details(int id)
	{
		var job = await _jobService.GetJobByIdAsync(id);

		if (job == null)
		{
			return NotFound();
		}

		return View(job);
	}

	[Authorize(Roles = AppRoles.Client)] // Only clients can create jobs
	public async Task<IActionResult> Create()
	{
		var viewModel = new CreateJobViewModel
		{
			AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>(),
			AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>(),
			Deadline = DateTime.Now.AddDays(30) // Default to 30 days from now
		};

		return View(viewModel);
	}

	[HttpPost]
	[Authorize(Roles = AppRoles.Client)] // Only clients can create jobs
	public async Task<IActionResult> Create(CreateJobViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			// Reload the dropdown data
			viewModel.AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>();
			viewModel.AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>();
			return View(viewModel);
		}

		// Validate budget range
		if (viewModel.BudgetMin > viewModel.BudgetMax)
		{
			ModelState.AddModelError("BudgetMax", "Maximum budget must be greater than or equal to minimum budget");
			viewModel.AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>();
			viewModel.AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>();
			return View(viewModel);
		}

		// Validate deadline
		if (viewModel.Deadline <= DateTime.Now)
		{
			ModelState.AddModelError("Deadline", "Deadline must be in the future");
			viewModel.AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>();
			viewModel.AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>();
			return View(viewModel);
		}

		var jobId = await _jobService.CreateJobAsync(viewModel);

		if (jobId.HasValue)
		{
			TempData["Success"] = "Job created successfully!";
			return RedirectToAction(nameof(Details), new { id = jobId.Value });
		}

		ModelState.AddModelError("", "Failed to create job. Please try again.");
		viewModel.AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>();
		viewModel.AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>();
		return View(viewModel);
	}

	[Authorize(Roles = AppRoles.Client)] // Only clients can edit their jobs
	public async Task<IActionResult> Edit(int id)
	{
		var job = await _jobService.GetJobByIdAsync(id);

		if (job == null)
		{
			return NotFound();
		}

		var viewModel = new UpdateJobViewModel
		{
			Id = job.Id,
			Title = job.Title ?? string.Empty,
			Description = job.Description ?? string.Empty,
			BudgetMin = job.BudgetMin,
			BudgetMax = job.BudgetMax,
			Deadline = job.Deadline,
			Tags = job.Tags,
			CategoryIds = job.Categories.Select(c => c.Id),
			SkillIds = job.Skills.Select(s => s.Id),
			AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>(),
			AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>()
		};

		return View(viewModel);
	}

	[HttpPost]
	[Authorize(Roles = AppRoles.Client)] // Only clients can edit their jobs
	public async Task<IActionResult> Edit(UpdateJobViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			// Reload the dropdown data
			viewModel.AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>();
			viewModel.AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>();
			return View(viewModel);
		}

		// Validate budget range
		if (viewModel.BudgetMin > viewModel.BudgetMax)
		{
			ModelState.AddModelError("BudgetMax", "Maximum budget must be greater than or equal to minimum budget");
			viewModel.AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>();
			viewModel.AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>();
			return View(viewModel);
		}

		// Validate deadline
		if (viewModel.Deadline <= DateTime.Now)
		{
			ModelState.AddModelError("Deadline", "Deadline must be in the future");
			viewModel.AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>();
			viewModel.AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>();
			return View(viewModel);
		}

		var success = await _jobService.UpdateJobAsync(viewModel);

		if (success)
		{
			TempData["Success"] = "Job updated successfully!";
			return RedirectToAction(nameof(Details), new { id = viewModel.Id });
		}

		ModelState.AddModelError("", "Failed to update job. Please try again.");
		viewModel.AvailableCategories = await _categoryService.GetAllCategoriesAsync() ?? new List<CategoryViewModel>();
		viewModel.AvailableSkills = await _skillService.GetAllSkillsAsync(isActive: true) ?? new List<SkillViewModel>();
		return View(viewModel);
	}

	[HttpPost]
	[Authorize(Roles = AppRoles.Client)] // Only clients can delete their jobs
	public async Task<IActionResult> Delete(int id)
	{
		var success = await _jobService.DeleteJobAsync(id);

		if (success)
		{
			TempData["Success"] = "Job deleted successfully!";
		}
		else
		{
			TempData["Error"] = "Failed to delete job. Please try again.";
		}

		return RedirectToAction(nameof(MyJobs));
	}

	[HttpPost]
	[Authorize(Roles = AppRoles.Client)] // Only clients can delete their jobs
	public async Task<IActionResult> ConfirmDelete(int id)
	{
		var job = await _jobService.GetJobByIdAsync(id);

		if (job == null)
		{
			return NotFound();
		}

		return PartialView("_ConfirmDelete", job);
	}
}