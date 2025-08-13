using FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractHistory;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetPendingChangeRequests;

public class GetPendingChangeRequestsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<GetPendingChangeRequestsQuery, GetPendingChangeRequestsResult>
{
    public async Task<GetPendingChangeRequestsResult> Handle(GetPendingChangeRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to view pending change requests");

        var userId = request.UserId ?? currentUserService.UserId!;
        
        var pendingRequests = await unitOfWork.ContractChangeRequests.GetUserPendingRequestsAsync(userId);

        var pendingRequestDtos = pendingRequests.Select(r => new PendingChangeRequestDto
        {
            Id = r.Id,
            RequestedByRole = r.RequestedByRole,
            ChangeDescription = r.ChangeDescription,
            Status = r.Status,
            RequestDate = r.RequestDate,
            ResponseByRole = r.ResponseByRole,
            ResponseDate = r.ResponseDate,
            ResponseNotes = r.ResponseNotes,
            ExpiryDate = r.ExpiryDate,
            FromVersion = MapToVersionDto(r.FromVersion),
            ProposedVersion = MapToVersionDto(r.ProposedVersion),
            ContractJobTitle = r.Contract?.Proposal?.Job?.Title ?? "Unknown Job",
            ClientName = r.Contract?.Client?.User?.FullName ?? "Unknown Client",
            FreelancerName = r.Contract?.Freelancer?.User?.FullName ?? "Unknown Freelancer",
            RequiresUserResponse = r.RequestedByUserId != userId && r.ResponseByUserId == null
        });

        return new GetPendingChangeRequestsResult
        {
            PendingRequests = pendingRequestDtos
        };
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
}