using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FreelanceJobBoard.Presentation.Controllers;

//[Authorize] 
public class JobsController : Controller
{
	private readonly JobService _jobService;
	private readonly CategoryService _categoryService;
	private readonly SkillService _skillService;
	private readonly ProposalService _proposalService;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ILogger<JobsController> _logger; // Add logger

	public JobsController(
		JobService jobService,
		CategoryService categoryService,
		SkillService skillService,
		ProposalService proposalService,
		IUnitOfWork unitOfWork,
		ILogger<JobsController> logger) // Inject logger
	{
		_jobService = jobService;
		_categoryService = categoryService;
		_skillService = skillService;
		_proposalService = proposalService;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}


	public async Task<IActionResult> Search(string query)
	{
		var jobs = await _jobService.SearchJobsAsync(query);
		return Ok(jobs);
	}

	public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, int? category = null, int? skill = null, string? sortDirection = null)
	{
		var jobs = await _jobService.GetAllJobsAsync(
			pageNumber: pageNumber, 
			pageSize: pageSize, 
			search: search, 
			sortBy: sortBy, 
			category: category, 
			skill: skill, 
			sortDirection: sortDirection);

		ViewBag.Search = search;
		ViewBag.SortBy = sortBy;
		ViewBag.SortDirection = sortDirection;
		ViewBag.Category = category;
		ViewBag.Skill = skill;

		return View(jobs);
	}

	public async Task<IActionResult> AllJobs(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, int? category = null, int? skill = null, string? sortDirection = null)
	{
		var jobs = await _jobService.GetAllJobsAsync(
			pageNumber: pageNumber, 
			pageSize: pageSize, 
			search: search, 
			sortBy: sortBy, 
			category: category, 
			skill: skill, 
			sortDirection: sortDirection);
			
		ViewBag.Search = search;
		ViewBag.SortBy = sortBy;
		ViewBag.SortDirection = sortDirection;
		ViewBag.Category = category;
		ViewBag.Skill = skill;

		var categories = await _categoryService.GetAllCategoriesAsync();
		ViewBag.Categories = categories.Select(c => new SelectListItem
		{
			Value = c.Id.ToString(),
			Text = c.Name,
			Selected = category.HasValue && category.Value == c.Id
		}).ToList();

		var skills = await _skillService.GetAllSkillsAsync(isActive: true);
		ViewBag.Skills = skills.Select(s => new SelectListItem
		{
			Value = s.Id.ToString(),
			Text = s.Name,
			Selected = skill.HasValue && skill.Value == s.Id
		}).ToList();

		return View(jobs);
	}

	public async Task<IActionResult> MyJobs()
	{
		var jobs = await _jobService.GetMyJobsAsync();
		return View(jobs);
	}
	[AllowAnonymous]

	public async Task<IActionResult> PublicJobDetails(int jobId)
	{
		var jobDetails = await _jobService.GetPublicJobDeatils(jobId);
		jobDetails.RelatedJobs = await _jobService.GetSimilarJobs(jobId);

		if (jobDetails == null)
		{
			return NotFound();
		}
		return View(jobDetails);
	}

	public async Task<IActionResult> Details(int id)
	{
		// Validate job ID
		if (id <= 0)
		{
			TempData["Error"] = "Invalid job ID provided.";
			return RedirectToAction(nameof(Index));
		}

		try
		{
			var job = await _jobService.GetJobByIdAsync(id);

			if (job == null)
			{
				_logger.LogWarning("Job not found in service layer | JobId={JobId}", id);
				TempData["Error"] = "The requested job could not be found. It may have been removed or you may not have permission to view it.";
				return RedirectToAction(nameof(Index));
			}

			// Set ownership information for role-based rendering
			ViewBag.IsOwner = false;
			ViewBag.HasFreelancerApplied = false;
			ViewBag.HasAcceptedProposal = false;
			ViewBag.IsAssignedFreelancer = false;
			ViewBag.CanReview = false;
			ViewBag.HasReviewed = false;
			ViewBag.ReviewType = string.Empty;

			var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (User.IsInRole(AppRoles.Client))
			{
				// Check if current user owns this job by comparing user ID with job's client ID
				try
				{
					var myJobs = await _jobService.GetMyJobsAsync();
					ViewBag.IsOwner = myJobs?.Any(j => j.Id == id) ?? false;

					// Check review capability for client
					if (ViewBag.IsOwner && job.Status == "Completed" && !string.IsNullOrEmpty(currentUserId))
					{
						var canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(id, currentUserId);
						var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedJobAsync(id, currentUserId);

						ViewBag.CanReview = canReview && !hasReviewed;
						ViewBag.HasReviewed = hasReviewed;
						ViewBag.ReviewType = ReviewType.ClientToFreelancer;
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Error checking job ownership for client | JobId={JobId}, UserId={UserId}", id, currentUserId);
					ViewBag.IsOwner = false;
				}
			}
			else if (User.IsInRole(AppRoles.Freelancer))
			{
				// Check if freelancer has already applied to this job
				try
				{
					ViewBag.HasFreelancerApplied = await _proposalService.HasFreelancerAppliedAsync(id);

					// Check if freelancer is assigned to this job
					if (!string.IsNullOrEmpty(currentUserId))
					{
						var freelancer = await _unitOfWork.Freelancers.GetByUserIdAsync(currentUserId);
						if (freelancer != null)
						{
							var jobWithDetails = await _unitOfWork.Jobs.GetJobWithDetailsAsync(id);
							ViewBag.IsAssignedFreelancer = jobWithDetails?.Proposals?.Any(p =>
								p.FreelancerId == freelancer.Id && p.Status == FreelanceJobBoard.Domain.Constants.ProposalStatus.Accepted) ?? false;
						}
					}

					// Check review capability for freelancer
					if (job.Status == "Completed" && !string.IsNullOrEmpty(currentUserId))
					{
						var canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(id, currentUserId);
						var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedJobAsync(id, currentUserId);

						ViewBag.CanReview = canReview && !hasReviewed;
						ViewBag.HasReviewed = hasReviewed;
						ViewBag.ReviewType = ReviewType.FreelancerToClient;
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Error checking freelancer job status | JobId={JobId}, UserId={UserId}", id, currentUserId);
					ViewBag.HasFreelancerApplied = false;
				}
			}

			// Check if job has an accepted proposal (for all users)
			try
			{
				ViewBag.HasAcceptedProposal = await _proposalService.HasJobAcceptedProposalAsync(id);
				_logger.LogDebug("Job acceptance status | JobId={JobId}, HasAcceptedProposal={HasAcceptedProposal}, JobStatus={JobStatus}", 
					id, (bool)ViewBag.HasAcceptedProposal, job.Status);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Error checking job acceptance status | JobId={JobId}", id);
				ViewBag.HasAcceptedProposal = false;
			}

			return View(job);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error loading job details | JobId={JobId}", id);
			TempData["Error"] = "An error occurred while loading the job details. Please try again.";
			return RedirectToAction(nameof(Index));
		}
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
			TempData["Success"] = "Job created successfully and submitted for admin approval! You'll be notified once it's reviewed.";

			// Try to verify the job was created before redirecting
			var createdJob = await _jobService.GetJobByIdAsync(jobId.Value);
			if (createdJob != null)
			{
				return RedirectToAction(nameof(Details), new { id = jobId.Value });
			}
			else
			{
				// Job was created but might not be immediately available (e.g., pending approval)
				// Redirect to MyJobs instead
				TempData["Info"] = "Your job has been created and is pending admin approval. You can view it in your jobs list.";
				return RedirectToAction(nameof(MyJobs));
			}
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

	[HttpPost]
	[Authorize] // Both clients and freelancers can mark jobs as complete
	public async Task<IActionResult> MarkAsCompleted(int id)
	{
		try
		{
			var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(currentUserId))
			{
				TempData["Error"] = "You must be logged in to complete a job.";
				return RedirectToAction(nameof(Details), new { id });
			}

			// Check if user can complete this job
			var canComplete = await _unitOfWork.Reviews.CanUserReviewJobAsync(id, currentUserId);
			if (!canComplete)
			{
				TempData["Error"] = "You don't have permission to complete this job.";
				return RedirectToAction(nameof(Details), new { id });
			}

			// Get the job to update its status
			var jobEntity = await _unitOfWork.Jobs.GetJobWithDetailsAsync(id);
			if (jobEntity == null)
			{
				TempData["Error"] = "Job not found.";
				return RedirectToAction(nameof(Index));
			}

			if (jobEntity.Status == JobStatus.Completed)
			{
				TempData["Info"] = "This job is already marked as completed.";
				return RedirectToAction(nameof(Details), new { id });
			}

			// Update job status to completed
			jobEntity.Status = JobStatus.Completed;
			jobEntity.CompletedDate = DateTime.UtcNow;

			_unitOfWork.Jobs.Update(jobEntity);
			await _unitOfWork.SaveChangesAsync();

			TempData["Success"] = "Job has been marked as completed! You can now leave a review.";
			return RedirectToAction(nameof(Details), new { id });
		}
		catch (Exception ex)
		{
			TempData["Error"] = "An error occurred while completing the job. Please try again.";
			return RedirectToAction(nameof(Details), new { id });
		}
	}
}