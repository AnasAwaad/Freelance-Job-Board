using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.RespondToChangeRequest;

public class RespondToChangeRequestCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationService notificationService, ILogger<RespondToChangeRequestCommandHandler> logger) : IRequestHandler<RespondToChangeRequestCommand>
{
    public async Task Handle(RespondToChangeRequestCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to respond to change requests");

        var changeRequest = await unitOfWork.ContractChangeRequests.GetRequestWithDetailsAsync(request.ChangeRequestId);
        if (changeRequest == null)
            throw new NotFoundException("ContractChangeRequest", request.ChangeRequestId.ToString());

        // Check if user has permission to respond
        var userRole = await GetUserRoleForContract(changeRequest.Contract);
        if (string.IsNullOrEmpty(userRole))
            throw new UnauthorizedAccessException("You don't have permission to respond to this change request");

        // Check if user is not the requester
        if (changeRequest.RequestedByUserId == currentUserService.UserId)
            throw new InvalidOperationException("You cannot respond to your own change request");

        // Check if request is still pending
        if (changeRequest.Status != Domain.Constants.ContractChangeRequestStatus.Pending)
            throw new InvalidOperationException("This change request has already been responded to");

        // Check if request has expired
        if (changeRequest.ExpiryDate.HasValue && changeRequest.ExpiryDate < DateTime.UtcNow)
        {
            changeRequest.Status = Domain.Constants.ContractChangeRequestStatus.Expired;
            await unitOfWork.SaveChangesAsync();
            throw new InvalidOperationException("This change request has expired");
        }

        // Update the change request
        changeRequest.Status = request.IsApproved ? Domain.Constants.ContractChangeRequestStatus.Approved : Domain.Constants.ContractChangeRequestStatus.Rejected;
        changeRequest.ResponseByUserId = currentUserService.UserId;
        changeRequest.ResponseByRole = userRole;
        changeRequest.ResponseDate = DateTime.UtcNow;
        changeRequest.ResponseNotes = request.ResponseNotes;
        changeRequest.LastUpdatedOn = DateTime.UtcNow;

        if (request.IsApproved)
        {
            // Make the proposed version the current version
            await unitOfWork.ContractVersions.UpdateCurrentVersionAsync(
                changeRequest.ContractId, 
                changeRequest.ProposedVersionId);
            
            logger.LogInformation("Contract version updated for contract {ContractId} to version {VersionId}", 
                changeRequest.ContractId, changeRequest.ProposedVersionId);
        }

        unitOfWork.ContractChangeRequests.Update(changeRequest);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Change request {RequestId} {Action} by user {UserId}", 
            request.ChangeRequestId, request.IsApproved ? "approved" : "rejected", currentUserService.UserId);

        // Send notification to the requester
        await NotifyRequester(changeRequest, request.IsApproved, request.ResponseNotes);
    }

    private async Task<string> GetUserRoleForContract(Domain.Entities.Contract contract)
    {
        var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
        if (client != null && contract.ClientId == client.Id)
            return Domain.Constants.UserRole.Client;

        var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
        if (freelancer != null && contract.FreelancerId == freelancer.Id)
            return Domain.Constants.UserRole.Freelancer;

        return string.Empty;
    }

    private async Task NotifyRequester(Domain.Entities.ContractChangeRequest changeRequest, bool isApproved, string? responseNotes)
    {
        try
        {
            var responderName = changeRequest.ResponseByRole == Domain.Constants.UserRole.Client 
                ? changeRequest.Contract.Client.User?.FullName 
                : changeRequest.Contract.Freelancer.User?.FullName;
            
            var title = $"Contract Change Request {(isApproved ? "Approved" : "Rejected")}: {changeRequest.Contract.Proposal?.Job?.Title}";
            var message = $"{responderName} has {(isApproved ? "approved" : "rejected")} your contract change request.";
            
            if (!string.IsNullOrEmpty(responseNotes))
            {
                message += $"\n\nResponse: {responseNotes}";
            }

            if (isApproved)
            {
                message += "\n\nThe contract has been updated with your proposed changes.";
            }

            await notificationService.CreateNotificationAsync(changeRequest.RequestedByUserId, title, message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send response notification for change request {RequestId}", changeRequest.Id);
        }
    }
}