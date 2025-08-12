using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using ContractStatusConstants = FreelanceJobBoard.Domain.Constants.ContractStatus;

namespace FreelanceJobBoard.Application.Features.Contracts.Commands.ProposeContractChange;

public class ProposeContractChangeCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationService notificationService, ICloudinaryService cloudinaryService, ILogger<ProposeContractChangeCommandHandler> logger) : IRequestHandler<ProposeContractChangeCommand, int>
{
    public async Task<int> Handle(ProposeContractChangeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting contract change proposal for contract {ContractId}", request.ContractId);

            if (!currentUserService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be authenticated to propose contract changes");

            var contract = await unitOfWork.Contracts.GetContractWithDetailsAsync(request.ContractId);
            if (contract == null)
                throw new NotFoundException(nameof(Contract), request.ContractId.ToString());

            logger.LogInformation("Contract {ContractId} found, checking user permissions", request.ContractId);

            // Check if user has permission to propose changes
            var userRole = await GetUserRoleForContract(contract);
            if (string.IsNullOrEmpty(userRole))
                throw new UnauthorizedAccessException("You don't have permission to propose changes to this contract");

            logger.LogInformation("User has {Role} role for contract {ContractId}", userRole, request.ContractId);

            // Check if contract is in a state that allows changes
            if (contract.ContractStatus?.Name == ContractStatusConstants.Completed || 
                contract.ContractStatus?.Name == ContractStatusConstants.Cancelled)
            {
                throw new InvalidOperationException("Cannot propose changes to a completed or cancelled contract");
            }

            // Check if there are pending change requests
            var hasPendingChanges = await unitOfWork.ContractChangeRequests.HasPendingChangesAsync(request.ContractId);
            if (hasPendingChanges)
            {
                throw new InvalidOperationException("There are already pending change requests for this contract. Please wait for them to be resolved.");
            }

            logger.LogInformation("Contract {ContractId} is eligible for changes, getting current version", request.ContractId);

            // Get current version
            var currentVersion = await unitOfWork.ContractVersions.GetCurrentVersionAsync(request.ContractId);
            if (currentVersion == null)
            {
                logger.LogInformation("No current version found, creating initial version for contract {ContractId}", request.ContractId);
                // Create initial version if none exists
                currentVersion = await CreateInitialVersion(contract);
            }

            logger.LogInformation("Current version: {VersionId} (version {VersionNumber}) for contract {ContractId}", 
                currentVersion.Id, currentVersion.VersionNumber, request.ContractId);

            // Create new proposed version
            var nextVersionNumber = await unitOfWork.ContractVersions.GetNextVersionNumberAsync(request.ContractId);
            logger.LogInformation("Next version number will be {VersionNumber} for contract {ContractId}", 
                nextVersionNumber, request.ContractId);

            var proposedVersion = new ContractVersion
            {
                ContractId = request.ContractId,
                VersionNumber = nextVersionNumber,
                Title = request.Title,
                Description = request.Description,
                PaymentAmount = request.PaymentAmount,
                PaymentType = request.PaymentType,
                ProjectDeadline = request.ProjectDeadline,
                Deliverables = request.Deliverables,
                TermsAndConditions = request.TermsAndConditions,
                AdditionalNotes = request.AdditionalNotes,
                CreatedByUserId = currentUserService.UserId!,
                CreatedByRole = userRole,
                CreatedOn = DateTime.UtcNow,
                IsCurrentVersion = false,
                ChangeReason = request.ChangeReason,
                IsActive = true
            };

            logger.LogInformation("Creating proposed version for contract {ContractId}", request.ContractId);
            await unitOfWork.ContractVersions.CreateAsync(proposedVersion);
            
            // Save changes to ensure the proposed version gets its ID
            await unitOfWork.SaveChangesAsync();
            
            logger.LogInformation("Proposed version created with ID {VersionId} for contract {ContractId}", 
                proposedVersion.Id, request.ContractId);

            // Verify that the proposed version has an ID
            if (proposedVersion.Id <= 0)
            {
                logger.LogError("Proposed version ID is invalid: {VersionId} for contract {ContractId}", 
                    proposedVersion.Id, request.ContractId);
                throw new InvalidOperationException("Failed to create proposed version - invalid ID generated");
            }

            // Create change request
            var changeRequest = new ContractChangeRequest
            {
                ContractId = request.ContractId,
                FromVersionId = currentVersion.Id,
                ProposedVersionId = proposedVersion.Id,
                RequestedByUserId = currentUserService.UserId!,
                RequestedByRole = userRole,
                ChangeDescription = request.ChangeReason,
                Status = Domain.Constants.ContractChangeRequestStatus.Pending,
                RequestDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7), // 7 days to respond
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            logger.LogInformation("Creating change request for contract {ContractId} from version {FromVersionId} to version {ProposedVersionId}", 
                request.ContractId, currentVersion.Id, proposedVersion.Id);

            await unitOfWork.ContractChangeRequests.CreateAsync(changeRequest);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Contract change request {ChangeRequestId} created for contract {ContractId} by user {UserId}", 
                changeRequest.Id, request.ContractId, currentUserService.UserId);

            // Send notification to the other party
            await NotifyOtherParty(contract, userRole, request.ChangeReason);

            return changeRequest.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while proposing contract changes for contract {ContractId}. Message: {Message}", 
                request.ContractId, ex.Message);
            throw;
        }
    }

    private async Task<string> GetUserRoleForContract(Contract contract)
    {
        try
        {
            var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
            if (client != null && contract.ClientId == client.Id)
                return Domain.Constants.UserRole.Client;

            var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
            if (freelancer != null && contract.FreelancerId == freelancer.Id)
                return Domain.Constants.UserRole.Freelancer;

            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user role for contract {ContractId}", contract.Id);
            return string.Empty;
        }
    }

    private async Task<ContractVersion> CreateInitialVersion(Contract contract)
    {
        try
        {
            var jobTitle = contract.Proposal?.Job?.Title ?? "Project";
            var jobDescription = contract.Proposal?.Job?.Description ?? "Project work as described in the original proposal";
            var coverLetter = contract.Proposal?.CoverLetter ?? "Work as agreed upon";

            var initialVersion = new ContractVersion
            {
                ContractId = contract.Id,
                VersionNumber = 1,
                Title = $"Initial Contract for {jobTitle}",
                Description = jobDescription,
                PaymentAmount = contract.PaymentAmount,
                PaymentType = contract.AgreedPaymentType ?? "Fixed",
                ProjectDeadline = null,
                Deliverables = coverLetter,
                TermsAndConditions = "Standard terms and conditions apply",
                AdditionalNotes = "Initial contract version created from accepted proposal",
                CreatedByUserId = "system",
                CreatedByRole = "System",
                // Use BaseEntity properties instead of overriding
                IsCurrentVersion = true,
                ChangeReason = "Initial contract creation",
                IsActive = true,
                CreatedOn = contract.CreatedOn // This is now properly inherited from BaseEntity
            };

            await unitOfWork.ContractVersions.CreateAsync(initialVersion);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Initial version {VersionId} created for contract {ContractId}", 
                initialVersion.Id, contract.Id);

            return initialVersion;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating initial version for contract {ContractId}", contract.Id);
            throw;
        }
    }

    private async Task NotifyOtherParty(Contract contract, string requesterRole, string changeReason)
    {
        try
        {
            var targetUserId = requesterRole == Domain.Constants.UserRole.Client 
                ? contract.Freelancer?.UserId 
                : contract.Client?.UserId;

            if (string.IsNullOrEmpty(targetUserId))
            {
                logger.LogWarning("Cannot send notification - target user ID is null for contract {ContractId}", contract.Id);
                return;
            }

            var jobTitle = contract.Proposal?.Job?.Title ?? "Project";
            
            // Use the dedicated contract change request notification method
            await notificationService.NotifyContractChangeRequestAsync(
                contract.Id,
                currentUserService.UserId!,
                targetUserId,
                jobTitle,
                changeReason
            );
            
            logger.LogInformation("Contract change request notification sent to user {UserId} for contract {ContractId}", 
                targetUserId, contract.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send contract change notification for contract {ContractId}", contract.Id);
            // Don't rethrow - notification failure shouldn't fail the main operation
        }
    }
}