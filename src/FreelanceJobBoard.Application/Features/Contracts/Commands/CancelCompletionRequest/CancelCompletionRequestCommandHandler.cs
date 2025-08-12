using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using ContractStatusConstants = FreelanceJobBoard.Domain.Constants.ContractStatus;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.CancelCompletionRequest;

public class CancelCompletionRequestCommandHandler(
    IUnitOfWork unitOfWork, 
    ICurrentUserService currentUserService, 
    INotificationService notificationService, 
    ILogger<CancelCompletionRequestCommandHandler> logger) : IRequestHandler<CancelCompletionRequestCommand>
{
    public async Task Handle(CancelCompletionRequestCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to cancel completion request");

        var contract = await unitOfWork.Contracts.GetContractWithDetailsAsync(request.ContractId);
        if (contract == null)
            throw new NotFoundException(nameof(Contract), request.ContractId.ToString());

        // Check if user has permission to cancel completion request
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
            throw new UnauthorizedAccessException("Only the client or freelancer involved in this contract can cancel completion request");

        // Validate current status
        var currentStatus = contract.ContractStatus.Name;
        if (currentStatus != ContractStatusConstants.PendingApproval)
            throw new InvalidOperationException("Contract must be pending approval to cancel completion request");

        // Verify the user is the one who requested completion
        if (contract.CompletionRequestedByUserId != currentUserService.UserId)
            throw new UnauthorizedAccessException("You can only cancel your own completion request");

        // Cancel completion request - set back to Active
        contract.ContractStatusId = 2; // Active
        contract.CompletionRequestedByUserId = null;
        contract.CompletionRequestedAt = null;
        contract.LastUpdatedOn = DateTime.UtcNow;

        unitOfWork.Contracts.Update(contract);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Contract {ContractId} completion request cancelled by {UserRole} {UserId}", 
            request.ContractId, userRole, currentUserService.UserId);

        try
        {
            // Notify both parties about the cancellation
            var otherPartyUserId = isClient ? contract.Freelancer.UserId! : contract.Client.UserId!;
            var title = $"Contract Completion Request Cancelled: {contract.Proposal.Job.Title}";
            var message = $"The {userRole.ToLower()} has cancelled their completion request. The contract is now active again.";
            
            if (!string.IsNullOrEmpty(request.Notes))
            {
                message += $"\n\nReason: {request.Notes}";
            }

            await notificationService.CreateNotificationAsync(contract.Client.UserId!, title, message);
            await notificationService.CreateNotificationAsync(contract.Freelancer.UserId!, title, message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send completion request cancellation notification for contract {ContractId}", contract.Id);
        }
    }
}