using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.UpdateProposalStatus;

public class UpdateProposalStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, INotificationService notificationService, ILogger<UpdateProposalStatusCommandHandler> logger) : IRequestHandler<UpdateProposalStatusCommand>
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

        // Additional validation: Check if trying to accept a proposal when another is already accepted
        if (request.Status == ProposalStatus.Accepted)
        {
            var allProposals = await unitOfWork.Proposals.GetAllAsync();
            var existingAcceptedProposal = allProposals
                .FirstOrDefault(p => p.JobId == proposal.JobId && 
                               p.Id != proposal.Id && 
                               p.Status == ProposalStatus.Accepted);

            if (existingAcceptedProposal != null)
            {
                throw new InvalidOperationException("This job already has an accepted proposal. Only one proposal can be accepted per job.");
            }

            // Check if a contract already exists for this proposal
            var existingContract = await unitOfWork.Contracts.GetContractByProposalIdAsync(request.ProposalId);
            if (existingContract != null)
            {
                throw new InvalidOperationException("A contract already exists for this proposal.");
            }
        }

        // Update the main proposal
        proposal.Status = request.Status;
        proposal.ClientFeedback = request.ClientFeedback;
        proposal.ReviewedAt = DateTime.UtcNow;
        proposal.ReviewedBy = client.Id;

        logger.LogInformation("Updated proposal {ProposalId} status to {Status}", proposal.Id, request.Status);

        if (request.Status == ProposalStatus.Accepted)
        {
            // Update job status
            job.Status = JobStatus.InProgress;
            logger.LogInformation("Updated job {JobId} status to InProgress", job.Id);
            
            // Create contract
            var contract = new Contract
            {
                ProposalId = proposal.Id,
                ClientId = proposal.ClientId ?? job.ClientId,
                FreelancerId = proposal.FreelancerId ?? throw new InvalidOperationException("Proposal must have a freelancer"),
                StartTime = DateTime.UtcNow,
                PaymentAmount = proposal.BidAmount,
                AgreedPaymentType = "Fixed", // Default to Fixed, can be enhanced later
                ContractStatusId = 1, // Pending status from seeded data
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            await unitOfWork.Contracts.CreateAsync(contract);
            logger.LogInformation("Created contract for proposal {ProposalId}", proposal.Id);
            
            // Get all proposals for this job and reject the others
            var allProposals = await unitOfWork.Proposals.GetAllAsync();
            var otherProposals = allProposals
                .Where(p => p.JobId == proposal.JobId && p.Id != proposal.Id && 
                           (p.Status == ProposalStatus.Submitted || p.Status == ProposalStatus.Pending || p.Status == ProposalStatus.UnderReview))
                .ToList();

            logger.LogInformation("Found {Count} other proposals to reject for job {JobId}", otherProposals.Count, proposal.JobId);

            foreach (var otherProposal in otherProposals)
            {
                logger.LogInformation("Rejecting proposal {ProposalId}", otherProposal.Id);
                
                otherProposal.Status = ProposalStatus.Rejected;
                otherProposal.ClientFeedback = "Job has been assigned to another freelancer";
                otherProposal.ReviewedAt = DateTime.UtcNow;
                otherProposal.ReviewedBy = client.Id;
                
                // Explicitly update the proposal to ensure change tracking
                unitOfWork.Proposals.Update(otherProposal);
            }
        }

        // Explicitly update the main proposal and job
        unitOfWork.Proposals.Update(proposal);
        if (request.Status == ProposalStatus.Accepted)
        {
            unitOfWork.Jobs.Update(job);
        }

        // Save all changes
        await unitOfWork.SaveChangesAsync();
        
        logger.LogInformation("Successfully saved all proposal status changes for job {JobId}", proposal.JobId);

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