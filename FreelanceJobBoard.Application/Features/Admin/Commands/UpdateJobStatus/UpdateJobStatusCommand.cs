using FreelanceJobBoard.Domain.Constants;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Admin.Commands.UpdateJobStatus;
public class UpdateJobStatusCommand(int jobId, JobStatus status, int approvedBy) : IRequest
{
	public int JobId { get; } = jobId;
	public JobStatus Status { get; } = status;
	public int ApprovedBy { get; } = approvedBy;
}
