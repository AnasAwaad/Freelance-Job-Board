using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.UpdateProposalStatus;

internal class UpdateProposalStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationService notificationService, ILogger<UpdateProposalStatusCommandHandler> logger) : IRequestHandler<UpdateProposalStatusCommand>
{
    public async Task Handle(UpdateProposalStatusCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to manage proposals");

        var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
        if (client == null)
            throw new NotFoundException("Client", currentUserService.UserId!);

        var proposal = await unitOfWork.Proposals.GetByIdAsync(request.ProposalId);
        if (proposal == null)
            throw new NotFoundException(nameof(Proposal), request.ProposalId.ToString());

        var job = await unitOfWork.Jobs.GetByIdAsync(proposal.JobId);
        if (job == null)
            throw new NotFoundException(nameof(Job), proposal.JobId.ToString());

        if (job.ClientId != client.Id)
            throw new UnauthorizedAccessException("Only the job owner can manage proposals for this job");

        var validStatuses = new[] { ProposalStatus.Accepted, ProposalStatus.Rejected, ProposalStatus.Pending, ProposalStatus.UnderReview };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");

        proposal.Status = request.Status;
        proposal.ClientFeedback = request.ClientFeedback;
        proposal.ReviewedAt = DateTime.UtcNow;
        proposal.ReviewedBy = client.Id;

        if (request.Status == ProposalStatus.Accepted)
        {
            job.Status = JobStatus.InProgress;
            
            var allProposals = await unitOfWork.Proposals.GetAllAsync();
            var otherProposals = allProposals
                .Where(p => p.JobId == proposal.JobId && p.Id != proposal.Id && 
                           (p.Status == ProposalStatus.Submitted || p.Status == ProposalStatus.Pending || p.Status == ProposalStatus.UnderReview))
                .ToList();

            foreach (var otherProposal in otherProposals)
            {
                otherProposal.Status = ProposalStatus.Rejected;
                otherProposal.ClientFeedback = "Job has been assigned to another freelancer";
                otherProposal.ReviewedAt = DateTime.UtcNow;
                otherProposal.ReviewedBy = client.Id;
            }
        }

        await unitOfWork.SaveChangesAsync();

        try
        {
            await notificationService.NotifyJobStatusChangeAsync(proposal.JobId, request.Status, request.ClientFeedback);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send job status change notification for job {JobId}", proposal.JobId);
        }
    }
}