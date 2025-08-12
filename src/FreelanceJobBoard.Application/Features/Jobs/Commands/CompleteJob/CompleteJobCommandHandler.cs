using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.CompleteJob;

public class CompleteJobCommandHandler : IRequestHandler<CompleteJobCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CompleteJobCommandHandler> _logger;

    public CompleteJobCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ILogger<CompleteJobCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(CompleteJobCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedAccessException("User must be authenticated to complete a job.");

        var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(request.JobId);
        if (job == null)
            throw new NotFoundException(nameof(Job), request.JobId.ToString());

        // Only the freelancer can mark job as complete initially
        var acceptedProposal = job.Proposals?.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
        if (acceptedProposal?.Freelancer?.UserId != currentUserId)
            throw new UnauthorizedAccessException("Only the assigned freelancer can mark the job as complete.");

        if (job.Status == JobStatus.Completed)
            throw new InvalidOperationException("Job is already completed.");

        if (job.Status != JobStatus.InProgress)
            throw new InvalidOperationException("Job must be in progress to be completed.");

        // Update job status to completed
        job.Status = JobStatus.Completed;
        job.CompletedDate = DateTime.UtcNow;
        
        // Add completion notes if provided
        if (!string.IsNullOrEmpty(request.CompletionNotes))
        {
            // Here you could add completion notes to a related entity if needed
        }

        _unitOfWork.Jobs.Update(job);
        await _unitOfWork.SaveChangesAsync();

        // Send email notifications to both client and freelancer about completion
        await SendJobCompletionNotifications(job, acceptedProposal.Freelancer);

        _logger.LogInformation("Job {JobId} has been marked as completed by freelancer {FreelancerId}", 
            request.JobId, currentUserId);

        return true;
    }

    private async Task SendJobCompletionNotifications(Job job, Freelancer freelancer)
    {
        try
        {
            // Notify client about job completion and invite them to review
            if (job.Client?.User?.Email != null)
            {
                var clientEmailData = new 
                {
                    ClientName = job.Client.User.FullName ?? "Client",
                    JobTitle = job.Title ?? "Unknown Job",
                    FreelancerName = freelancer.User?.FullName ?? "Freelancer",
                    JobId = job.Id,
                    ReviewUrl = $"/reviews/create?jobId={job.Id}&type=ClientToFreelancer",
                    CompletionDate = job.CompletedDate?.ToString("MMMM dd, yyyy") ?? DateTime.UtcNow.ToString("MMMM dd, yyyy")
                };

                await _emailService.SendTemplateEmailAsync(
                    job.Client.User.Email,
                    "JobCompletedClientNotification",
                    clientEmailData);
            }

            // Notify freelancer about completion and invite them to review
            if (freelancer.User?.Email != null)
            {
                var freelancerEmailData = new 
                {
                    FreelancerName = freelancer.User.FullName ?? "Freelancer",
                    JobTitle = job.Title ?? "Unknown Job",
                    ClientName = job.Client?.User?.FullName ?? "Client",
                    JobId = job.Id,
                    ReviewUrl = $"/reviews/create?jobId={job.Id}&type=FreelancerToClient",
                    CompletionDate = job.CompletedDate?.ToString("MMMM dd, yyyy") ?? DateTime.UtcNow.ToString("MMMM dd, yyyy")
                };

                await _emailService.SendTemplateEmailAsync(
                    freelancer.User.Email,
                    "JobCompletedFreelancerNotification",
                    freelancerEmailData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send job completion notifications for job {JobId}", job.Id);
            // Don't throw here to avoid breaking the completion process
        }
    }
}