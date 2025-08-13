using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using ContractStatusConstants = FreelanceJobBoard.Domain.Constants.ContractStatus;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.RequestCompletion;

public class RequestCompletionCommandHandler(
    IUnitOfWork unitOfWork, 
    ICurrentUserService currentUserService, 
    INotificationService notificationService, 
    ILogger<RequestCompletionCommandHandler> logger) : IRequestHandler<RequestCompletionCommand>
{
    public async Task Handle(RequestCompletionCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to request contract completion");

        var contract = await unitOfWork.Contracts.GetContractWithDetailsAsync(request.ContractId);
        if (contract == null)
            throw new NotFoundException(nameof(Contract), request.ContractId.ToString());

        // Check if user has permission to request completion
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
            throw new UnauthorizedAccessException("Only the client or freelancer involved in this contract can request completion");

        // Validate current status
        var currentStatus = contract.ContractStatus.Name;
        if (currentStatus != ContractStatusConstants.Active)
            throw new InvalidOperationException("Contract must be active to request completion");

        // Update contract status to pending approval
        contract.ContractStatusId = 3; // PendingApproval
        contract.LastUpdatedOn = DateTime.UtcNow;
        contract.CompletionRequestedByUserId = currentUserService.UserId;
        contract.CompletionRequestedAt = DateTime.UtcNow;

        // Store who requested completion in notes
        var completionNotes = $"Completion requested by: {userRole}";
        if (!string.IsNullOrEmpty(request.Notes))
        {
            completionNotes += $". Notes: {request.Notes}";
        }

        unitOfWork.Contracts.Update(contract);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Contract {ContractId} completion requested by {UserRole} {UserId}", 
            request.ContractId, userRole, currentUserService.UserId);

        try
        {
            // Notify the other party about the completion request
            var otherPartyUserId = isClient ? contract.Freelancer.UserId! : contract.Client.UserId!;
            var otherPartyRole = isClient ? "Freelancer" : "Client";
            
            var title = $"Contract Completion Request: {contract.Proposal.Job.Title}";
            var message = $"The {userRole.ToLower()} has requested to mark this contract as complete. Please review and approve or provide feedback.";
            
            if (!string.IsNullOrEmpty(request.Notes))
            {
                message += $"\n\nCompletion Notes: {request.Notes}";
            }

            await notificationService.CreateNotificationAsync(otherPartyUserId, title, message);
            
            // Also notify the requester
            var requesterUserId = isClient ? contract.Client.UserId! : contract.Freelancer.UserId!;
            var requesterMessage = $"Your completion request for this contract has been sent to the {otherPartyRole.ToLower()} for approval.";
            await notificationService.CreateNotificationAsync(requesterUserId, title, requesterMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send completion request notification for contract {ContractId}", contract.Id);
        }
    }
}