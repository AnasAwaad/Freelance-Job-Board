using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using ContractStatusConstants = FreelanceJobBoard.Domain.Constants.ContractStatus;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.UpdateContractStatus;

public class UpdateContractStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationService notificationService, ILogger<UpdateContractStatusCommandHandler> logger) : IRequestHandler<UpdateContractStatusCommand>
{
    public async Task Handle(UpdateContractStatusCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to manage contracts");

        var contract = await unitOfWork.Contracts.GetContractWithDetailsAsync(request.ContractId);
        if (contract == null)
            throw new NotFoundException(nameof(Contract), request.ContractId.ToString());

        // Check if user has permission to update this contract
        var isClient = false;
        var isFreelancer = false;
        
        var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
        if (client != null && contract.ClientId == client.Id)
        {
            isClient = true;
        }
        
        var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
        if (freelancer != null && contract.FreelancerId == freelancer.Id)
        {
            isFreelancer = true;
        }

        if (!isClient && !isFreelancer)
            throw new UnauthorizedAccessException("Only the client or freelancer involved in this contract can update its status");

        var validStatuses = new[] { ContractStatusConstants.Pending, ContractStatusConstants.Active, ContractStatusConstants.Completed, ContractStatusConstants.Cancelled };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");

        // Business rules for status transitions
        var currentStatus = contract.ContractStatus.Name;
        
        // Validate status transitions
        if (currentStatus == ContractStatusConstants.Completed || currentStatus == ContractStatusConstants.Cancelled)
        {
            throw new InvalidOperationException($"Cannot change status from {currentStatus}");
        }

        if (request.Status == ContractStatusConstants.Active && currentStatus != ContractStatusConstants.Pending)
        {
            throw new InvalidOperationException("Contract can only be activated from Pending status");
        }

        if (request.Status == ContractStatusConstants.Completed && currentStatus != ContractStatusConstants.Active)
        {
            throw new InvalidOperationException("Contract can only be completed from Active status");
        }

        // Update contract status
        var contractStatusId = request.Status switch
        {
            ContractStatusConstants.Pending => 1,
            ContractStatusConstants.Active => 2,
            ContractStatusConstants.Completed => 3,
            ContractStatusConstants.Cancelled => 4,
            _ => throw new ArgumentException("Invalid contract status")
        };

        contract.ContractStatusId = contractStatusId;
        contract.LastUpdatedOn = DateTime.UtcNow;

        // Set end time if completing or cancelling
        if (request.Status == ContractStatusConstants.Completed || request.Status == ContractStatusConstants.Cancelled)
        {
            contract.EndTime = DateTime.UtcNow;
        }

        // Update related job status
        var job = contract.Proposal.Job;
        if (request.Status == ContractStatusConstants.Completed)
        {
            job.Status = JobStatus.Completed;
            unitOfWork.Jobs.Update(job);
            logger.LogInformation("Updated job {JobId} status to Completed", job.Id);
        }
        else if (request.Status == ContractStatusConstants.Cancelled)
        {
            job.Status = JobStatus.Cancelled;
            unitOfWork.Jobs.Update(job);
            logger.LogInformation("Updated job {JobId} status to Cancelled", job.Id);
        }

        unitOfWork.Contracts.Update(contract);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Updated contract {ContractId} status to {Status}", contract.Id, request.Status);

        try
        {
            // Notify both parties about the status change
            var title = $"Contract Status Updated: {contract.Proposal.Job.Title}";
            var message = $"Contract status has been updated to: {request.Status}";
            if (!string.IsNullOrEmpty(request.Notes))
            {
                message += $"\n\nNotes: {request.Notes}";
            }

            await notificationService.CreateNotificationAsync(contract.Client.UserId!, title, message);
            await notificationService.CreateNotificationAsync(contract.Freelancer.UserId!, title, message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send contract status change notification for contract {ContractId}", contract.Id);
        }
    }
}