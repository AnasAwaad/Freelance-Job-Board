using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
internal class UpdateJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<UpdateJobCommand>
{
	public async Task Handle(UpdateJobCommand request, CancellationToken cancellationToken)
	{

		var job = await unitOfWork.Jobs.GetJobWithCategoriesAndSkillsAsync(request.Id);

		if (job is null)
			throw new NotFoundException(nameof(Job), request.Id.ToString());
		mapper.Map(request, job);


		if (request.CategoryIds is not null && request.CategoryIds.Any())
		{
			var categories = await unitOfWork.Categories.GetCategoriesByIdsAsync(request.CategoryIds);

			job.Categories = new List<JobCategory>();

			foreach (var category in categories)
			{
				job.Categories.Add(new JobCategory { CategoryId = category.Id });
			}
		}


		if (request.SkillIds is not null && request.SkillIds.Any())
		{
			var skills = await unitOfWork.Skills.GetSkillsByIdsAsync(request.SkillIds);

			job.Skills = new List<JobSkill>();

			foreach (var skill in skills)
			{
				job.Skills.Add(new JobSkill { SkillId = skill.Id });
			}
		}

		await unitOfWork.SaveChangesAsync();
	}
}
