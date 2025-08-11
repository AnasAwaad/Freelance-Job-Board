using FreelanceJobBoard.Application.Features.Contracts.Commands.ProposeContractChange;
using FreelanceJobBoard.Application.Features.Contracts.Commands.RespondToChangeRequest;
using FreelanceJobBoard.Application.Features.Contracts.Commands.UpdateContractStatus;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractDetails;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractHistory;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetPendingChangeRequests;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetUserContracts;
using FreelanceJobBoard.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(IMediator mediator, ILogger<ContractsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get contracts for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserContracts([FromQuery] string? userId = null)
    {
        try
        {
            var query = new GetUserContractsQuery { UserId = userId ?? string.Empty };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user contracts");
            return StatusCode(500, "An error occurred while retrieving contracts");
        }
    }

    /// <summary>
    /// Get contract details by ID
    /// </summary>
    [HttpGet("{contractId}")]
    public async Task<IActionResult> GetContractDetails(int contractId)
    {
        try
        {
            var query = new GetContractDetailsQuery { ContractId = contractId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Domain.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract details for contract {ContractId}", contractId);
            return StatusCode(500, "An error occurred while retrieving contract details");
        }
    }

    /// <summary>
    /// Get contract version history
    /// </summary>
    [HttpGet("{contractId}/history")]
    public async Task<IActionResult> GetContractHistory(int contractId)
    {
        try
        {
            var query = new GetContractHistoryQuery { ContractId = contractId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Domain.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contract history for contract {ContractId}", contractId);
            return StatusCode(500, "An error occurred while retrieving contract history");
        }
    }

    /// <summary>
    /// Get pending change requests for the current user
    /// </summary>
    [HttpGet("pending-changes")]
    public async Task<IActionResult> GetPendingChangeRequests([FromQuery] string? userId = null)
    {
        try
        {
            var query = new GetPendingChangeRequestsQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending change requests");
            return StatusCode(500, "An error occurred while retrieving pending change requests");
        }
    }

    /// <summary>
    /// Debug endpoint to check contract information before proposing changes
    /// </summary>
    [HttpGet("{contractId}/debug")]
    public async Task<IActionResult> DebugContract(int contractId)
    {
        try
        {
            _logger.LogInformation("Debug request for contract {ContractId}", contractId);
            
            var userId = User.Identity?.Name ?? "unknown";
            _logger.LogInformation("Current user: {UserId}", userId);
            
            return Ok(new { 
                contractId, 
                userId, 
                userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                message = "Debug info retrieved successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in debug endpoint for contract {ContractId}", contractId);
            return StatusCode(500, new { message = "Debug failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Propose changes to a contract
    /// </summary>
    [HttpPost("{contractId}/propose-changes")]
    public async Task<IActionResult> ProposeContractChanges(int contractId, [FromForm] ProposeContractChangeRequest request)
    {
        try
        {
            _logger.LogInformation("=== Starting contract change proposal ===");
            _logger.LogInformation("Contract ID: {ContractId}", contractId);
            _logger.LogInformation("User Claims: {@Claims}", User.Claims.Select(c => new { c.Type, c.Value }).ToList());
            _logger.LogInformation("Request data: {@Request}", request);

            if (request == null)
            {
                _logger.LogWarning("ProposeContractChanges called with null request for contract {ContractId}", contractId);
                return BadRequest(new { message = "Request body is required" });
            }

            // Validate required fields
            if (string.IsNullOrEmpty(request.Title))
            {
                return BadRequest(new { message = "Title is required" });
            }

            if (string.IsNullOrEmpty(request.Description))
            {
                return BadRequest(new { message = "Description is required" });
            }

            if (request.PaymentAmount <= 0)
            {
                return BadRequest(new { message = "Payment amount must be greater than 0" });
            }

            if (string.IsNullOrEmpty(request.PaymentType))
            {
                return BadRequest(new { message = "Payment type is required" });
            }

            if (string.IsNullOrEmpty(request.ChangeReason))
            {
                return BadRequest(new { message = "Change reason is required" });
            }

            var command = new ProposeContractChangeCommand
            {
                ContractId = contractId,
                Title = request.Title,
                Description = request.Description,
                PaymentAmount = request.PaymentAmount,
                PaymentType = request.PaymentType,
                ProjectDeadline = request.ProjectDeadline,
                Deliverables = request.Deliverables,
                TermsAndConditions = request.TermsAndConditions,
                AdditionalNotes = request.AdditionalNotes,
                ChangeReason = request.ChangeReason,
                AttachmentFiles = request.AttachmentFiles ?? new List<IFormFile>()
            };

            _logger.LogInformation("Sending ProposeContractChangeCommand: {@Command}", command);

            var changeRequestId = await _mediator.Send(command);
            
            _logger.LogInformation("=== Contract change proposal completed successfully ===");
            _logger.LogInformation("Created change request with ID: {ChangeRequestId}", changeRequestId);
            
            return Ok(new { changeRequestId, message = "Contract change request submitted successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempting to propose changes for contract {ContractId}", contractId);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Domain.Exceptions.NotFoundException ex)
        {
            _logger.LogWarning(ex, "Contract {ContractId} not found when proposing changes", contractId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when proposing changes for contract {ContractId}: {Message}", contractId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when proposing changes for contract {ContractId}: {Message}", contractId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== UNEXPECTED ERROR IN CONTRACT CHANGE PROPOSAL ===");
            _logger.LogError(ex, "Contract ID: {ContractId}", contractId);
            _logger.LogError(ex, "Request: {@Request}", request);
            _logger.LogError(ex, "Exception Type: {ExceptionType}", ex.GetType().FullName);
            _logger.LogError(ex, "Exception Message: {Message}", ex.Message);
            _logger.LogError(ex, "Stack Trace: {StackTrace}", ex.StackTrace);
            
            return StatusCode(500, new { 
                message = "An error occurred while proposing contract changes", 
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    /// <summary>
    /// Respond to a contract change request
    /// </summary>
    [HttpPost("change-requests/{changeRequestId}/respond")]
    public async Task<IActionResult> RespondToChangeRequest(int changeRequestId, [FromBody] RespondToChangeRequestRequest request)
    {
        try
        {
            var command = new RespondToChangeRequestCommand
            {
                ChangeRequestId = changeRequestId,
                IsApproved = request.IsApproved,
                ResponseNotes = request.ResponseNotes
            };

            await _mediator.Send(command);
            return Ok(new { message = $"Change request {(request.IsApproved ? "approved" : "rejected")} successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Domain.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to change request {ChangeRequestId}", changeRequestId);
            return StatusCode(500, "An error occurred while responding to the change request");
        }
    }

    /// <summary>
    /// Update contract status
    /// </summary>
    [HttpPut("{contractId}/status")]
    public async Task<IActionResult> UpdateContractStatus(int contractId, [FromBody] UpdateContractStatusRequest request)
    {
        try
        {
            var command = new UpdateContractStatusCommand
            {
                ContractId = contractId,
                Status = request.Status,
                Notes = request.Notes
            };

            await _mediator.Send(command);
            return Ok(new { message = "Contract status updated successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Domain.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract status for contract {ContractId}", contractId);
            return StatusCode(500, "An error occurred while updating contract status");
        }
    }

    /// <summary>
    /// Start contract (change status from Pending to Active)
    /// </summary>
    [HttpPost("{contractId}/start")]
    public async Task<IActionResult> StartContract(int contractId, [FromBody] ContractActionRequest? request = null)
    {
        try
        {
            var command = new UpdateContractStatusCommand
            {
                ContractId = contractId,
                Status = ContractStatus.Active,
                Notes = request?.Notes
            };

            await _mediator.Send(command);
            return Ok(new { message = "Contract started successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Domain.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting contract {ContractId}", contractId);
            return StatusCode(500, "An error occurred while starting the contract");
        }
    }

    /// <summary>
    /// Complete contract (change status from Active to Completed)
    /// </summary>
    [HttpPost("{contractId}/complete")]
    public async Task<IActionResult> CompleteContract(int contractId, [FromBody] ContractActionRequest? request = null)
    {
        try
        {
            var command = new UpdateContractStatusCommand
            {
                ContractId = contractId,
                Status = ContractStatus.Completed,
                Notes = request?.Notes
            };

            await _mediator.Send(command);
            return Ok(new { message = "Contract completed successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Domain.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing contract {ContractId}", contractId);
            return StatusCode(500, "An error occurred while completing the contract");
        }
    }

    /// <summary>
    /// Cancel contract
    /// </summary>
    [HttpPost("{contractId}/cancel")]
    public async Task<IActionResult> CancelContract(int contractId, [FromBody] ContractActionRequest? request = null)
    {
        try
        {
            var command = new UpdateContractStatusCommand
            {
                ContractId = contractId,
                Status = ContractStatus.Cancelled,
                Notes = request?.Notes
            };

            await _mediator.Send(command);
            return Ok(new { message = "Contract cancelled successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Domain.Exceptions.NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling contract {ContractId}", contractId);
            return StatusCode(500, "An error occurred while cancelling the contract");
        }
    }
}

public class ProposeContractChangeRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTime? ProjectDeadline { get; set; }
    public string? Deliverables { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? AdditionalNotes { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public List<IFormFile> AttachmentFiles { get; set; } = new List<IFormFile>();
}

public class RespondToChangeRequestRequest
{
    public bool IsApproved { get; set; }
    public string? ResponseNotes { get; set; }
}

public class UpdateContractStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ContractActionRequest
{
    public string? Notes { get; set; }
}