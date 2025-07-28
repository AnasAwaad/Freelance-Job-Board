using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
namespace FreelanceJobBoard.Application.Features.Jobs.Commands.ApproveJob;
internal class ApproveJobCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<ApproveJobCommandHandler> logger) : IRequestHandler<ApproveJobCommand>
{
    public async Task Handle(ApproveJobCommand request, CancellationToken cancellationToken)
    {
        var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId);
        
        if (job == null)
            throw new NotFoundException(nameof(Job), request.JobId.ToString());

        job.IsApproved = request.IsApproved;
        job.ApprovedBy = int.TryParse(request.AdminUserId, out var adminId) ? adminId : null;
        job.LastUpdatedOn = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        try
        {
            await notificationService.NotifyJobApprovalAsync(request.JobId, request.IsApproved, request.AdminMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send job approval notification for job {JobId}", request.JobId);
        }
    }
}