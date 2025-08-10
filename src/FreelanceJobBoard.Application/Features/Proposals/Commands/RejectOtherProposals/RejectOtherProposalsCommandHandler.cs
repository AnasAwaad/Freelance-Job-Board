using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.RejectOtherProposals;

public class RejectOtherProposalsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<RejectOtherProposalsCommandHandler> logger) : IRequestHandler<RejectOtherProposalsCommand>
{
    public async Task Handle(RejectOtherProposalsCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to manage proposals");

        var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
        if (client == null)
            throw new NotFoundException("Client", currentUserService.UserId!);

        var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId);
        if (job == null)
            throw new NotFoundException("Job", request.JobId.ToString());

        if (job.ClientId != client.Id)
            throw new UnauthorizedAccessException("Only the job owner can manage proposals for this job");

        // Get all proposals for the job
        var allProposals = await unitOfWork.Proposals.GetAllAsync();
        var jobProposals = allProposals
            .Where(p => p.JobId == request.JobId && p.Id != request.AcceptedProposalId)
            .Where(p => p.Status == ProposalStatus.Submitted || p.Status == ProposalStatus.Pending || p.Status == ProposalStatus.UnderReview)
            .ToList();

        logger.LogInformation("Rejecting {Count} other proposals for job {JobId}", jobProposals.Count, request.JobId);

        foreach (var proposal in jobProposals)
        {
            proposal.Status = ProposalStatus.Rejected;
            proposal.ClientFeedback = "Job has been assigned to another freelancer";
            proposal.ReviewedAt = DateTime.UtcNow;
            proposal.ReviewedBy = client.Id;
            
            unitOfWork.Proposals.Update(proposal);
            logger.LogInformation("Rejected proposal {ProposalId}", proposal.Id);
        }

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation("Successfully rejected {Count} proposals for job {JobId}", jobProposals.Count, request.JobId);
    }
}