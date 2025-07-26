using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.DeleteJob;
public class DeleteJobCommandHandler(IMapper mapper, IUnitOfWork unitOfWork) : IRequestHandler<DeleteJobCommand>
{
	public async Task Handle(DeleteJobCommand request, CancellationToken cancellationToken)
	{
		var job = await unitOfWork.Jobs.GetJobWithCategoriesAndSkillsAsync(request.Id);

		if (job is null)
			throw new NotFoundException(nameof(Job), request.Id.ToString());


		unitOfWork.JobCategories.RemoveRange(job.Categories);
		unitOfWork.JobSkills.RemoveRange(job.Skills);
		unitOfWork.Jobs.Delete(job);

		await unitOfWork.SaveChangesAsync();
	}
}
