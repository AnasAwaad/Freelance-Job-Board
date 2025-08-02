using MediatR;

namespace FreelanceJobBoard.Application.Features.Admin.Commands.UpdateJobStatus;
public class UpdateJobStatusCommand(int jobId, string status, string adminUserId, string? message) : IRequest
{
	public int JobId { get; } = jobId;
	public string Status { get; } = status;
	public string? AdminMessage { get; } = message;
	public string AdminUserId { get; } = adminUserId;
}
