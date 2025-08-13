using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractDetails;

public class GetContractDetailsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<GetContractDetailsQueryHandler> logger) : IRequestHandler<GetContractDetailsQuery, ContractDetailsDto>
{
    public async Task<ContractDetailsDto> Handle(GetContractDetailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting contract details for contract {ContractId}", request.ContractId);

            if (!currentUserService.IsAuthenticated)
                throw new UnauthorizedAccessException("User must be authenticated to view contract details");

            var contract = await unitOfWork.Contracts.GetContractWithDetailsAsync(request.ContractId);
            if (contract == null)
                throw new NotFoundException(nameof(Contract), request.ContractId.ToString());

            logger.LogInformation("Contract {ContractId} found, checking user permissions", request.ContractId);

            // Check if user has permission to view this contract
            var hasPermission = false;
            
            try
            {
                var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
                if (client != null && contract.ClientId == client.Id)
                {
                    hasPermission = true;
                    logger.LogInformation("User has client permissions for contract {ContractId}", request.ContractId);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error checking client permissions for contract {ContractId}", request.ContractId);
            }
            
            if (!hasPermission)
            {
                try
                {
                    var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
                    if (freelancer != null && contract.FreelancerId == freelancer.Id)
                    {
                        hasPermission = true;
                        logger.LogInformation("User has freelancer permissions for contract {ContractId}", request.ContractId);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error checking freelancer permissions for contract {ContractId}", request.ContractId);
                }
            }

            if (!hasPermission)
                throw new UnauthorizedAccessException("You don't have permission to view this contract");

            // Get current version for versioned fields
            ContractVersion? currentVersion = null;
            try
            {
                currentVersion = await unitOfWork.ContractVersions.GetCurrentVersionAsync(request.ContractId);
                logger.LogInformation("Current version found for contract {ContractId}: {VersionId}", 
                    request.ContractId, currentVersion?.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error getting current version for contract {ContractId}", request.ContractId);
            }
            
            // Build the contract details DTO
            var contractDetails = new ContractDetailsDto
            {
                Id = contract.Id,
                ProposalId = contract.ProposalId,
                JobTitle = contract.Proposal?.Job?.Title ?? "Unknown Job",
                JobDescription = contract.Proposal?.Job?.Description ?? "",
                ClientName = contract.Client?.User?.FullName ?? "Unknown Client",
                ClientEmail = contract.Client?.User?.Email ?? "",
                FreelancerName = contract.Freelancer?.User?.FullName ?? "Unknown Freelancer",
                FreelancerEmail = contract.Freelancer?.User?.Email ?? "",
                StartTime = contract.StartTime,
                EndTime = contract.EndTime,
                ContractStatus = contract.ContractStatus?.Name ?? "Unknown",
                ContractStatusId = contract.ContractStatusId,
                CreatedOn = contract.CreatedOn,
                LastUpdatedOn = contract.LastUpdatedOn,
                CoverLetter = contract.Proposal?.CoverLetter ?? "",
                EstimatedTimelineDays = contract.Proposal?.EstimatedTimelineDays ?? 0,
                CompletionRequestedByUserId = contract.CompletionRequestedByUserId,
                CompletionRequestedAt = contract.CompletionRequestedAt
            };

            // Use current version data if available, otherwise fallback to contract data
            if (currentVersion != null)
            {
                contractDetails.PaymentAmount = currentVersion.PaymentAmount;
                contractDetails.AgreedPaymentType = currentVersion.PaymentType;
                
                // Add version-specific details
                contractDetails.Title = currentVersion.Title;
                contractDetails.Description = currentVersion.Description;
                contractDetails.ProjectDeadline = currentVersion.ProjectDeadline;
                contractDetails.Deliverables = currentVersion.Deliverables;
                contractDetails.TermsAndConditions = currentVersion.TermsAndConditions;
                contractDetails.AdditionalNotes = currentVersion.AdditionalNotes;
                contractDetails.CurrentVersionNumber = currentVersion.VersionNumber;
                // Use the most recent time between creation and last update from BaseEntity
                contractDetails.LastVersionUpdateDate = currentVersion.LastUpdatedOn ?? currentVersion.CreatedOn;
                
                logger.LogInformation("Using current version data for contract {ContractId}, version {VersionNumber}", 
                    request.ContractId, currentVersion.VersionNumber);
            }
            else
            {
                // Fallback to original contract data
                contractDetails.PaymentAmount = contract.PaymentAmount;
                contractDetails.AgreedPaymentType = contract.AgreedPaymentType;
                contractDetails.CurrentVersionNumber = 1;
                // Use the most recent time between creation and last update
                contractDetails.LastVersionUpdateDate = contract.LastUpdatedOn ?? contract.CreatedOn;
                
                logger.LogInformation("Using fallback contract data for contract {ContractId} (no version found)", request.ContractId);
            }

            logger.LogInformation("Successfully retrieved contract details for contract {ContractId}", request.ContractId);
            return contractDetails;
        }
        catch (Exception ex)
        {
            // Log the full exception details for debugging
            logger.LogError(ex, "Error in GetContractDetailsQueryHandler for contract {ContractId}: {Message}", 
                request.ContractId, ex.Message);
            throw;
        }
    }
}