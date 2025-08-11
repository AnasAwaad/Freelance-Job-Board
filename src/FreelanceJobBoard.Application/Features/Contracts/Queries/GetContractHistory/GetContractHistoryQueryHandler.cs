using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractHistory;

public class GetContractHistoryQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<GetContractHistoryQuery, GetContractHistoryResult>
{
    public async Task<GetContractHistoryResult> Handle(GetContractHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to view contract history");

        var contract = await unitOfWork.Contracts.GetContractWithDetailsAsync(request.ContractId);
        if (contract == null)
            throw new NotFoundException(nameof(Domain.Entities.Contract), request.ContractId.ToString());

        // Check if user has permission to view this contract
        var hasPermission = await CheckUserPermission(contract);
        if (!hasPermission)
            throw new UnauthorizedAccessException("You don't have permission to view this contract history");

        // Get current version
        var currentVersion = await unitOfWork.ContractVersions.GetCurrentVersionAsync(request.ContractId);
        
        // Get version history
        var versionHistory = await unitOfWork.ContractVersions.GetVersionHistoryAsync(request.ContractId);
        
        // Get change requests
        var changeRequests = await unitOfWork.ContractChangeRequests.GetRequestHistoryAsync(request.ContractId);

        return new GetContractHistoryResult
        {
            ContractId = request.ContractId,
            CurrentVersion = currentVersion != null ? MapToVersionDto(currentVersion) : CreateDefaultVersion(contract),
            VersionHistory = versionHistory.Select(MapToVersionDto),
            ChangeRequests = changeRequests.Select(MapToChangeRequestDto)
        };
    }

    private async Task<bool> CheckUserPermission(Domain.Entities.Contract contract)
    {
        var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
        if (client != null && contract.ClientId == client.Id)
            return true;

        var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
        if (freelancer != null && contract.FreelancerId == freelancer.Id)
            return true;

        return false;
    }

    private static ContractVersionDto MapToVersionDto(Domain.Entities.ContractVersion version)
    {
        return new ContractVersionDto
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            Title = version.Title,
            Description = version.Description,
            PaymentAmount = version.PaymentAmount,
            PaymentType = version.PaymentType,
            ProjectDeadline = version.ProjectDeadline,
            Deliverables = version.Deliverables,
            TermsAndConditions = version.TermsAndConditions,
            AdditionalNotes = version.AdditionalNotes,
            CreatedByUserId = version.CreatedByUserId,
            CreatedByRole = version.CreatedByRole,
            CreatedOn = version.CreatedOn,
            IsCurrentVersion = version.IsCurrentVersion,
            ChangeReason = version.ChangeReason
        };
    }

    private static ContractChangeRequestDto MapToChangeRequestDto(Domain.Entities.ContractChangeRequest request)
    {
        return new ContractChangeRequestDto
        {
            Id = request.Id,
            RequestedByRole = request.RequestedByRole,
            ChangeDescription = request.ChangeDescription,
            Status = request.Status,
            RequestDate = request.RequestDate,
            ResponseByRole = request.ResponseByRole,
            ResponseDate = request.ResponseDate,
            ResponseNotes = request.ResponseNotes,
            ExpiryDate = request.ExpiryDate,
            FromVersion = MapToVersionDto(request.FromVersion),
            ProposedVersion = MapToVersionDto(request.ProposedVersion)
        };
    }

    private static ContractVersionDto CreateDefaultVersion(Domain.Entities.Contract contract)
    {
        return new ContractVersionDto
        {
            Id = 0,
            VersionNumber = 1,
            Title = $"Initial Contract for {contract.Proposal?.Job?.Title ?? "Project"}",
            Description = contract.Proposal?.Job?.Description ?? "Project work as described in the original proposal",
            PaymentAmount = contract.PaymentAmount,
            PaymentType = contract.AgreedPaymentType ?? "Fixed",
            ProjectDeadline = null,
            Deliverables = contract.Proposal?.CoverLetter,
            TermsAndConditions = "Standard terms and conditions apply",
            AdditionalNotes = "Initial contract version created from accepted proposal",
            CreatedByUserId = "system",
            CreatedByRole = "System",
            CreatedOn = contract.CreatedOn,
            IsCurrentVersion = true,
            ChangeReason = "Initial contract creation"
        };
    }
}