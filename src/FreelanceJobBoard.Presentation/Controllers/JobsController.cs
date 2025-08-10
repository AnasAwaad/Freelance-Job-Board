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
    private readonly ProposalService _proposalService;

    public JobsController(JobService jobService, CategoryService categoryService, SkillService skillService, ProposalService proposalService)
    {
        _jobService = jobService;
        _categoryService = categoryService;
        _skillService = skillService;
        _proposalService = proposalService;
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

    public async Task<IActionResult> Details(int id)
    {
        // Validate job ID
        if (id <= 0)
        {
            TempData["Error"] = "Invalid job ID provided.";
            return RedirectToAction(nameof(Index));
        }

        var job = await _jobService.GetJobByIdAsync(id);
        
        if (job == null)
        {
            TempData["Error"] = "The requested job could not be found. It may have been removed or you may not have permission to view it.";
            return RedirectToAction(nameof(Index));
        }

        // Set ownership information for role-based rendering
        ViewBag.IsOwner = false;
        ViewBag.HasFreelancerApplied = false;
        ViewBag.HasAcceptedProposal = false;
        
        if (User.IsInRole(AppRoles.Client))
        {
            // Check if current user owns this job by comparing user ID with job's client ID
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            // Note: In a real implementation, you would need to get the current user's client ID
            // For now, we'll assume ownership based on whether they can see their jobs
            try
            {
                var myJobs = await _jobService.GetMyJobsAsync();
                ViewBag.IsOwner = myJobs?.Any(j => j.Id == id) ?? false;
            }
            catch
            {
                ViewBag.IsOwner = false;
            }
        }
        else if (User.IsInRole(AppRoles.Freelancer))
        {
            // Check if freelancer has already applied to this job
            try
            {
                ViewBag.HasFreelancerApplied = await _proposalService.HasFreelancerAppliedAsync(id);
            }
            catch
            {
                ViewBag.HasFreelancerApplied = false;
            }
        }

        // Check if job has an accepted proposal (for all users)
        try
        {
            ViewBag.HasAcceptedProposal = await _proposalService.HasJobAcceptedProposalAsync(id);
        }
        catch
        {
            ViewBag.HasAcceptedProposal = false;
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
}