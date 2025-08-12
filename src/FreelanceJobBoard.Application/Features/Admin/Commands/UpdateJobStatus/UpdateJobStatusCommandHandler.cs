using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Admin.Commands.UpdateJobStatus;
internal class UpdateJobStatusCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<UpdateJobStatusCommandHandler> logger) : IRequestHandler<UpdateJobStatusCommand>
{
	public async Task Handle(UpdateJobStatusCommand request, CancellationToken cancellationToken)
	{
		logger.LogInformation("🔄 Starting job status update | JobId={JobId}, NewStatus={NewStatus}, AdminUserId={AdminUserId}", 
			request.JobId, request.Status, request.AdminUserId);

		var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId);
		
		if (job == null)
		{
			logger.LogWarning("❌ Job not found for status update | JobId={JobId}, AdminUserId={AdminUserId}", 
				request.JobId, request.AdminUserId);
			throw new NotFoundException(nameof(Job), request.JobId.ToString());
		}

		var previousStatus = job.Status;
		logger.LogDebug("📊 Job status details | JobId={JobId}, PreviousStatus={PreviousStatus}, NewStatus={NewStatus}, ClientId={ClientId}", 
			request.JobId, previousStatus, request.Status, job.ClientId);

		job.Status = request.Status;

		if (job.Status == JobStatus.Open)
		{
			job.ApprovedBy = request.AdminUserId;
			logger.LogDebug("✅ Job approved | JobId={JobId}, ApprovedBy={AdminUserId}", request.JobId, request.AdminUserId);
		}
		else if (job.Status == JobStatus.Cancelled)
		{
			job.RejectedBy = request.AdminUserId;
			logger.LogDebug("❌ Job rejected | JobId={JobId}, RejectedBy={AdminUserId}", request.JobId, request.AdminUserId);
		}

		job.LastUpdatedOn = DateTime.UtcNow;

		logger.LogDebug("💾 Saving job status update to database | JobId={JobId}", request.JobId);
		await unitOfWork.SaveChangesAsync();

		logger.LogInformation("✅ Job status updated successfully | JobId={JobId}, Status={Status}, UpdatedBy={AdminUserId}", 
			request.JobId, request.Status, request.AdminUserId);

		try
		{
			logger.LogDebug("📨 Sending job approval notification | JobId={JobId}, IsApproved={IsApproved}", 
				request.JobId, request.Status == JobStatus.Open);
			
			await notificationService.NotifyJobApprovalAsync(request.JobId, request.Status == JobStatus.Open, request.AdminMessage);
			
			logger.LogDebug("✅ Job approval notification sent successfully | JobId={JobId}", request.JobId);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "❌ Failed to send job approval notification | JobId={JobId}, Status={Status}", 
				request.JobId, request.Status);
		}

		logger.LogInformation("🎉 Job status update process completed | JobId={JobId}, FinalStatus={Status}", 
			request.JobId, request.Status);
	}
}
