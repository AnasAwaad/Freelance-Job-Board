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
		var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId)
			?? throw new NotFoundException(nameof(Job), request.JobId.ToString());

		job.Status = request.Status;

		if (job.Status == JobStatus.Open)
			job.ApprovedBy = request.AdminUserId;
		else if (job.Status == JobStatus.Cancelled)
			job.RejectedBy = request.AdminUserId;


		job.LastUpdatedOn = DateTime.UtcNow;

		await unitOfWork.SaveChangesAsync();

		try
		{
			await notificationService.NotifyJobApprovalAsync(request.JobId, request.Status == JobStatus.Open, request.AdminMessage);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to send job approval notification for job {JobId}", request.JobId);
		}
	}
}
