using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Admin.Commands.UpdateJobStatus;
internal class UpdateJobStatusCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateJobStatusCommand>
{
	public async Task Handle(UpdateJobStatusCommand request, CancellationToken cancellationToken)
	{
		var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId)
			?? throw new NotFoundException(nameof(Job), request.JobId.ToString());

		job.Status = request.Status;
		job.ApprovedBy = request.ApprovedBy;

		await unitOfWork.SaveChangesAsync();
	}
}
