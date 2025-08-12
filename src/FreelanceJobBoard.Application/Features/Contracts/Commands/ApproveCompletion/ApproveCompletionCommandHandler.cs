using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using ContractStatusConstants = FreelanceJobBoard.Domain.Constants.ContractStatus;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.ApproveCompletion;

public class ApproveCompletionCommandHandler(
    IUnitOfWork unitOfWork, 
    ICurrentUserService currentUserService, 
    INotificationService notificationService, 
    ILogger<ApproveCompletionCommandHandler> logger) : IRequestHandler<ApproveCompletionCommand>
{
    public async Task Handle(ApproveCompletionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to approve contract completion");

        var contract = await unitOfWork.Contracts.GetContractWithDetailsAsync(request.ContractId);
        if (contract == null)
            throw new NotFoundException(nameof(Contract), request.ContractId.ToString());

        // Check if user has permission to approve completion
        var isClient = false;
        var isFreelancer = false;
        var userRole = "";
        
        var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
        if (client != null && contract.ClientId == client.Id)
        {
            isClient = true;
            userRole = "Client";
        }
        
        var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
        if (freelancer != null && contract.FreelancerId == freelancer.Id)
        {
            isFreelancer = true;
            userRole = "Freelancer";
        }

        if (!isClient && !isFreelancer)
            throw new UnauthorizedAccessException("Only the client or freelancer involved in this contract can approve completion");

        // Validate current status
        var currentStatus = contract.ContractStatus.Name;
        if (currentStatus != ContractStatusConstants.PendingApproval)
            throw new InvalidOperationException("Contract must be pending approval to approve or reject completion");

        // Verify the user is not the one who requested completion
        if (contract.CompletionRequestedByUserId == currentUserService.UserId)
            throw new InvalidOperationException("You cannot approve your own completion request. Only the other party can approve completion");

        if (request.IsApproved)
        {
            // Approve completion - set to Completed
            contract.ContractStatusId = 4; // Completed
            contract.EndTime = DateTime.UtcNow;
            
            // Update related job status
            var job = contract.Proposal.Job;
            job.Status = JobStatus.Completed;
            unitOfWork.Jobs.Update(job);
            logger.LogInformation("Updated job {JobId} status to Completed", job.Id);
        }
        else
        {
            // Reject completion - set back to Active
            contract.ContractStatusId = 2; // Active
        }

        // Clear completion request tracking
        contract.CompletionRequestedByUserId = null;
        contract.CompletionRequestedAt = null;
        contract.LastUpdatedOn = DateTime.UtcNow;

        unitOfWork.Contracts.Update(contract);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Contract {ContractId} completion {Action} by {UserRole} {UserId}", 
            request.ContractId, request.IsApproved ? "approved" : "rejected", userRole, currentUserService.UserId);

        try
        {
            // Notify both parties about the approval/rejection
            var action = request.IsApproved ? "approved" : "rejected";
            var title = $"Contract Completion {action.ToUpper()}: {contract.Proposal.Job.Title}";
            
            string message;
            if (request.IsApproved)
            {
                message = $"The {userRole.ToLower()} has approved the contract completion. The contract is now marked as completed.";
            }
            else
            {
                message = $"The {userRole.ToLower()} has rejected the completion request. The contract remains active.";
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    message += $"\n\nReason: {request.Notes}";
                }
            }

            await notificationService.CreateNotificationAsync(contract.Client.UserId!, title, message);
            await notificationService.CreateNotificationAsync(contract.Freelancer.UserId!, title, message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send completion approval notification for contract {ContractId}", contract.Id);
        }
    }
}