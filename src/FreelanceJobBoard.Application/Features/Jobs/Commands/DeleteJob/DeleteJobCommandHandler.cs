using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.DeleteJob;
public class DeleteJobCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<DeleteJobCommand>
{
	public async Task Handle(DeleteJobCommand request, CancellationToken cancellationToken)
	{
		// Get the current authenticated user
		if (!currentUserService.IsAuthenticated)
			throw new UnauthorizedAccessException("User must be authenticated to delete a job");

		var job = await unitOfWork.Jobs.GetJobWithCategoriesAndSkillsAsync(request.Id);

		if (job is null)
			throw new NotFoundException(nameof(Job), request.Id.ToString());

		// Get the client associated with the current user
		var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
		if (client == null)
			throw new NotFoundException("Client", currentUserService.UserId!);

		// Ensure only the job creator can delete the job
		if (job.ClientId != client.Id)
			throw new UnauthorizedAccessException("Only the job creator can delete this job");

		unitOfWork.JobCategories.RemoveRange(job.Categories);
		unitOfWork.JobSkills.RemoveRange(job.Skills);
		unitOfWork.Jobs.Delete(job);

		await unitOfWork.SaveChangesAsync();
	}
}
