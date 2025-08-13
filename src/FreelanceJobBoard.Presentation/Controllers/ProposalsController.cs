using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize]
public class ProposalsController : Controller
{
	private readonly ProposalService _proposalService;
	private readonly JobService _jobService;
	private readonly ILogger<ProposalsController> _logger;

	public ProposalsController(
		ProposalService proposalService,
		JobService jobService,
		ILogger<ProposalsController> logger)
	{
		_proposalService = proposalService;
		_jobService = jobService;
		_logger = logger;
	}

	// Freelancer views their proposals
	[HttpGet]
	[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> MyProposals()
	{
		try
		{
			var proposals = await _proposalService.GetFreelancerProposalsAsync();
			return View(proposals);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading freelancer proposals");
			TempData["Error"] = "Unable to load your proposals. Please try again.";
			return View(new List<ProposalViewModel>());
		}
	}

	// Client views proposals for their job
	[HttpGet]
	[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> JobProposals(int jobId, string? status = null)
	{
		try
		{
			var job = await _jobService.GetJobByIdAsync(jobId);
			if (job == null)
			{
				TempData["Error"] = "Job not found.";
				return RedirectToAction("MyJobs", "Jobs");
			}

			var proposals = await _proposalService.GetJobProposalsAsync(jobId, status);

			ViewBag.Job = job;
			ViewBag.CurrentStatus = status;

			return View(proposals);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading job proposals for job {JobId}", jobId);
			TempData["Error"] = "Unable to load proposals. Please try again.";
			return RedirectToAction("MyJobs", "Jobs");
		}
	}

	// Freelancer submits a proposal
	[HttpGet]
	[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> Submit(int jobId)
	{
		try
		{
			var job = await _jobService.GetJobByIdAsync(jobId);
			if (job == null)
			{
				TempData["Error"] = "Job not found.";
				return RedirectToAction("Index", "Jobs");
			}

			if (job.Status != "Open")
			{
				TempData["Error"] = "This job is no longer accepting proposals.";
				return RedirectToAction("Details", "Jobs", new { id = jobId });
			}

			// Check if freelancer has already applied
			var hasApplied = await _proposalService.HasFreelancerAppliedAsync(jobId);
			if (hasApplied)
			{
				TempData["Error"] = "You have already submitted a proposal for this job.";
				return RedirectToAction("Details", "Jobs", new { id = jobId });
			}

			// Check if job already has an accepted proposal
			var hasAcceptedProposal = await _proposalService.HasJobAcceptedProposalAsync(jobId);
			if (hasAcceptedProposal)
			{
				TempData["Error"] = "This job has already been assigned to another freelancer.";
				return RedirectToAction("Details", "Jobs", new { id = jobId });
			}

			var viewModel = new SubmitProposalViewModel
			{
				JobId = jobId,
				JobTitle = job.Title ?? "Unknown",
				JobDescription = job.Description ?? "",
				BudgetMin = job.BudgetMin,
				BudgetMax = job.BudgetMax,
				Deadline = job.Deadline,
				ClientAverageRating = job.ClientAverageRating,
				ClientTotalReviews = job.ClientTotalReviews
			};

			return View(viewModel);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error submit job proposals for job {JobId}", jobId);
			TempData["Error"] = "Unable to Submit proposals. Please try again.";
			return RedirectToAction("MyJobs", "Jobs");
		}

	}

	[ValidateAntiForgeryToken]
	[HttpPost]
	[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> Submit(SubmitProposalViewModel viewModel)
	{
		if (!ModelState.IsValid)
		{
			try
			{
				var job = await _jobService.GetJobByIdAsync(viewModel.JobId);
				if (job != null)
				{
					viewModel.JobTitle = job.Title ?? "Unknown";
					viewModel.JobDescription = job.Description ?? "";
					viewModel.BudgetMin = job.BudgetMin;
					viewModel.BudgetMax = job.BudgetMax;
					viewModel.Deadline = job.Deadline;
				}
			}
			catch { }

			return View(viewModel);
		}

		try
		{
			var success = await _proposalService.SubmitProposalAsync(viewModel);

			if (success)
			{
				TempData["Success"] = "Your proposal has been submitted successfully!";
				return RedirectToAction("MyProposals");
			}
			else
			{
				ModelState.AddModelError("", "Failed to submit proposal. Please try again.");
				return View(viewModel);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error submitting proposal for job {JobId}", viewModel.JobId);
			ModelState.AddModelError("", "An error occurred while submitting your proposal. Please try again.");
			return View(viewModel);
		}
	}

	// Client updates proposal status (approve/reject)
	[HttpPost]
	[Authorize(Roles = AppRoles.Client)]
	public async Task<IActionResult> UpdateStatus(int proposalId, string status, string? feedback = null)
	{
		// Validate the status value
		var validStatuses = new[] { ProposalStatus.Accepted, ProposalStatus.Rejected, ProposalStatus.Pending, ProposalStatus.UnderReview };
		if (!validStatuses.Contains(status))
		{
			TempData["Error"] = "Invalid proposal status provided.";
			return RedirectToAction("MyJobs", "Jobs");
		}

		try
		{
			var result = await _proposalService.UpdateProposalStatusAsync(proposalId, status, feedback);

			if (result.IsSuccess)
			{
				var message = status == ProposalStatus.Accepted ? "Proposal accepted successfully! All other proposals have been automatically rejected." : "Proposal rejected successfully!";
				TempData["Success"] = message;

				// If accepting a proposal, call the additional rejection endpoint to ensure all other proposals are rejected
				if (status == ProposalStatus.Accepted)
				{
					var jobIdStr = Request.Form["jobId"].FirstOrDefault();
					if (!string.IsNullOrEmpty(jobIdStr) && int.TryParse(jobIdStr, out var targetJobId))
					{
						try
						{
							var rejectResult = await _proposalService.RejectOtherProposalsAsync(targetJobId, proposalId);
							if (!rejectResult.IsSuccess)
							{
								_logger.LogWarning("Failed to reject other proposals for job {JobId}: {Error}", targetJobId, rejectResult.ErrorMessage);
							}
						}
						catch (Exception ex)
						{
							_logger.LogWarning(ex, "Error rejecting other proposals for job {JobId}", targetJobId);
						}
					}

					// Increased delay to ensure all changes are processed and database is consistent
					await Task.Delay(1000); // Increased from 500ms to 1000ms
					
					_logger.LogInformation("? Proposal {ProposalId} accepted successfully, job status should now be InProgress", proposalId);
				}
			}
			else
			{
				// Handle validation errors
				if (result.ValidationErrors != null && result.ValidationErrors.Any())
				{
					var validationMessages = result.ValidationErrors
						.SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}"))
						.ToList();

					TempData["Error"] = string.Join("; ", validationMessages);
				}
				else
				{
					// Handle general errors
					TempData["Error"] = result.ErrorMessage ?? "Failed to update proposal status. Please try again.";
				}

				_logger.LogWarning("Failed to update proposal {ProposalId} status. Error: {Error}, StatusCode: {StatusCode}",
					proposalId, result.ErrorMessage, result.StatusCode);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating proposal status for proposal {ProposalId}", proposalId);
			TempData["Error"] = "An error occurred while updating the proposal. Please try again.";
		}

		// Get the job ID from the form or try to determine it from the referrer
		var jobId = Request.Form["jobId"].FirstOrDefault();
		if (!string.IsNullOrEmpty(jobId) && int.TryParse(jobId, out var parsedJobId))
		{
			return RedirectToAction("JobProposals", new { jobId = parsedJobId });
		}

		// Fallback to MyJobs if we can't determine the job ID
		return RedirectToAction("MyJobs", "Jobs");
	}

	// Freelancer views proposal details
	[HttpGet]
	[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> Details(int id)
	{
		try
		{
			var proposal = await _proposalService.GetProposalDetailsAsync(id);

			if (proposal == null)
			{
				TempData["Error"] = "Proposal not found.";
				return RedirectToAction("MyProposals");
			}

			return View(proposal);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading proposal details for proposal {ProposalId}", id);
			TempData["Error"] = "Unable to load proposal details. Please try again.";
			return RedirectToAction("MyProposals");
		}
	}

	// Freelancer deletes their proposal
	[HttpPost]
	[Authorize(Roles = AppRoles.Freelancer)]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var success = await _proposalService.DeleteProposalAsync(id);

			if (success)
			{
				TempData["Success"] = "Proposal deleted successfully!";
			}
			else
			{
				TempData["Error"] = "Failed to delete proposal. Please try again.";
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting proposal {ProposalId}", id);
			TempData["Error"] = "An error occurred while deleting the proposal. Please try again.";
		}

		return RedirectToAction("MyProposals");
	}
}
