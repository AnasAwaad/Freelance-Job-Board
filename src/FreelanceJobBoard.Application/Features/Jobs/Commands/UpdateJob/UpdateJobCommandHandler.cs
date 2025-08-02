using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Application.Tests.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
public class UpdateJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<UpdateJobCommand>
{
	public async Task Handle(UpdateJobCommand request, CancellationToken cancellationToken)
	{
		var job = await unitOfWork.Jobs.GetJobWithCategoriesAndSkillsAsync(request.Id);

		if (job is null)
			throw new NotFoundException(nameof(Job), request.Id.ToString());

		// Get the client associated with the current user
		var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
		if (client == null)
			throw new NotFoundException("Client", currentUserService.UserId!);

		// Ensure only the job creator can update the job
		if (job.ClientId != client.Id)
			throw new UnauthorizedException("Only the job creator can update this job");

		mapper.Map(request, job);


		if (request.CategoryIds is not null && request.CategoryIds.Any())
		{
			var categories = await unitOfWork.Categories.GetCategoriesByIdsAsync(request.CategoryIds);


			if (categories.Count != request.CategoryIds.Count())
				throw new MissingCategoriesException("Some selected categories could not be found.");

			job.Categories = new List<JobCategory>();

			foreach (var category in categories)
			{
				job.Categories.Add(new JobCategory { CategoryId = category.Id });
			}
		}


		if (request.SkillIds is not null && request.SkillIds.Any())
		{
			var skills = await unitOfWork.Skills.GetSkillsByIdsAsync(request.SkillIds);


			if (skills.Count != request.SkillIds.Count())
				throw new MissingSkillsException("Some selected skills could not be found.");


			job.Skills = new List<JobSkill>();

			foreach (var skill in skills)
			{
				job.Skills.Add(new JobSkill { SkillId = skill.Id });
			}
		}

		await unitOfWork.SaveChangesAsync();
	}
}
