using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
internal class CreateJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateJobCommand>
{
	public async Task Handle(CreateJobCommand request, CancellationToken cancellationToken)
	{
		var job = mapper.Map<Job>(request);

		if (request.CategoryIds is not null && request.CategoryIds.Any())
		{
			var categories = await unitOfWork.Categories.GetCategoriesByIdsAsync(request.CategoryIds);

			foreach (var category in categories)
			{
				job.Categories.Add(new JobCategory { CategoryId = category.Id });
			}
		}


		if (request.SkillIds is not null && request.SkillIds.Any())
		{
			var skills = await unitOfWork.Skills.GetSkillsByIdsAsync(request.SkillIds);

			foreach (var skill in skills)
			{
				job.Skills.Add(new JobSkill { SkillId = skill.Id });
			}
		}

		await unitOfWork.Jobs.CreateAsync(job);
		await unitOfWork.SaveChangesAsync();


	}
}
