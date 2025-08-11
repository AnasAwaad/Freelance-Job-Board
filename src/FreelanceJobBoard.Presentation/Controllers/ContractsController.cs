using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private readonly ContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(ContractService contractService, ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var contracts = await _contractService.GetUserContractsAsync();
            return View(contracts ?? new Application.Features.Contracts.Queries.GetUserContracts.GetUserContractsResult());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading contracts");
            TempData["Error"] = "An error occurred while loading your contracts. Please try again.";
            return View(new Application.Features.Contracts.Queries.GetUserContracts.GetUserContractsResult());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var contract = await _contractService.GetContractDetailsAsync(id);
            if (contract == null)
            {
                TempData["Error"] = "Contract not found.";
                return RedirectToAction("Index");
            }
            
            // Enable debug mode if in development or if debug parameter is passed
            ViewBag.IsDebugMode = HttpContext.Request.Query.ContainsKey("debug");
            
            return View(contract);
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You don't have permission to view this contract.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading contract details for contract {ContractId}", id);
            TempData["Error"] = "An error occurred while loading the contract details. Please try again.";
            return RedirectToAction("Index");
        }
    }

    public async Task<IActionResult> History(int id)
    {
        try
        {
            var history = await _contractService.GetContractHistoryAsync(id);
            if (history == null)
            {
                TempData["Error"] = "Contract not found.";
                return RedirectToAction("Index");
            }
            return View(history);
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You don't have permission to view this contract history.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading contract history for contract {ContractId}", id);
            TempData["Error"] = "An error occurred while loading the contract history. Please try again.";
            return RedirectToAction("Index");
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var contract = await _contractService.GetContractDetailsAsync(id);
            if (contract == null)
            {
                TempData["Error"] = "Contract not found.";
                return RedirectToAction("Index");
            }

            // Check if contract can be edited
            if (contract.ContractStatus == "Completed" || contract.ContractStatus == "Cancelled")
            {
                TempData["Error"] = "Cannot edit a completed or cancelled contract.";
                return RedirectToAction("Details", new { id });
            }

            // Use current version data if available, otherwise fallback to original contract data
            var viewModel = new ProposeContractChangeViewModel
            {
                ContractId = contract.Id,
                Title = contract.Title ?? $"Contract for {contract.JobTitle}",
                Description = contract.Description ?? contract.JobDescription,
                PaymentAmount = contract.PaymentAmount,
                PaymentType = contract.AgreedPaymentType ?? "Fixed",
                ProjectDeadline = contract.ProjectDeadline,
                Deliverables = contract.Deliverables ?? contract.CoverLetter,
                TermsAndConditions = contract.TermsAndConditions ?? "Standard terms and conditions apply",
                AdditionalNotes = contract.AdditionalNotes ?? "",
                ChangeReason = ""
            };

            return View(viewModel);
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You don't have permission to edit this contract.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading contract for editing {ContractId}", id);
            TempData["Error"] = "An error occurred while loading the contract for editing. Please try again.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProposeContractChangeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _contractService.ProposeContractChangesAsync(model.ContractId, model);
            
            if (result.IsSuccess)
            {
                TempData["Success"] = "Contract change proposal submitted successfully! The other party will be notified to review your changes.";
                return RedirectToAction("Details", new { id = model.ContractId });
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to propose contract changes.";
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while proposing contract changes for contract {ContractId}", model.ContractId);
            TempData["Error"] = "An error occurred while proposing contract changes. Please try again.";
            return View(model);
        }
    }

    public async Task<IActionResult> PendingChanges()
    {
        try
        {
            var pendingChanges = await _contractService.GetPendingChangeRequestsAsync();
            return View(pendingChanges ?? new Application.Features.Contracts.Queries.GetPendingChangeRequests.GetPendingChangeRequestsResult());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading pending change requests");
            TempData["Error"] = "An error occurred while loading pending change requests. Please try again.";
            return View(new Application.Features.Contracts.Queries.GetPendingChangeRequests.GetPendingChangeRequestsResult());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondToChange(int changeRequestId, bool isApproved, string? responseNotes = null)
    {
        try
        {
            var result = await _contractService.RespondToChangeRequestAsync(changeRequestId, isApproved, responseNotes);
            
            if (result.IsSuccess)
            {
                var message = isApproved 
                    ? "Contract changes approved successfully! The contract has been updated." 
                    : "Contract changes rejected successfully.";
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to respond to change request.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while responding to change request {ChangeRequestId}", changeRequestId);
            TempData["Error"] = "An error occurred while responding to the change request. Please try again.";
        }

        return RedirectToAction("PendingChanges");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int contractId, string status, string? notes = null)
    {
        var validStatuses = new[] { ContractStatus.Pending, ContractStatus.Active, ContractStatus.Completed, ContractStatus.Cancelled };
        if (!validStatuses.Contains(status))
        {
            TempData["Error"] = "Invalid contract status provided.";
            return RedirectToAction("Details", new { id = contractId });
        }

        try
        {
            var result = await _contractService.UpdateContractStatusAsync(contractId, status, notes);
            
            if (result.IsSuccess)
            {
                var statusMessage = status switch
                {
                    ContractStatus.Active => "Contract activated successfully!",
                    ContractStatus.Completed => "Contract completed successfully!",
                    ContractStatus.Cancelled => "Contract cancelled successfully!",
                    _ => "Contract status updated successfully!"
                };
                TempData["Success"] = statusMessage;
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to update contract status.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating contract status for contract {ContractId}", contractId);
            TempData["Error"] = "An error occurred while updating the contract status. Please try again.";
        }

        return RedirectToAction("Details", new { id = contractId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int contractId, string? notes = null)
    {
        try
        {
            var result = await _contractService.StartContractAsync(contractId, notes);
            
            if (result.IsSuccess)
            {
                TempData["Success"] = "Contract started successfully!";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to start contract.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting contract {ContractId}", contractId);
            TempData["Error"] = "An error occurred while starting the contract. Please try again.";
        }

        return RedirectToAction("Details", new { id = contractId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int contractId, string? notes = null)
    {
        try
        {
            var result = await _contractService.CompleteContractAsync(contractId, notes);
            
            if (result.IsSuccess)
            {
                TempData["Success"] = "Contract completed successfully! The job is now closed.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to complete contract.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing contract {ContractId}", contractId);
            TempData["Error"] = "An error occurred while completing the contract. Please try again.";
        }

        return RedirectToAction("Details", new { id = contractId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int contractId, string? notes = null)
    {
        try
        {
            var result = await _contractService.CancelContractAsync(contractId, notes);
            
            if (result.IsSuccess)
            {
                TempData["Success"] = "Contract cancelled successfully.";
            }
            else
            {
                TempData["Error"] = result.ErrorMessage ?? "Failed to cancel contract.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling contract {ContractId}", contractId);
            TempData["Error"] = "An error occurred while cancelling the contract. Please try again.";
        }

        return RedirectToAction("Details", new { id = contractId });
    }
}