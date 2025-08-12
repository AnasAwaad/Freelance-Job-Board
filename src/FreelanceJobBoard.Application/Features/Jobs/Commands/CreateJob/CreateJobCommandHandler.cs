using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Application.Tests.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
public class CreateJobCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, INotificationService notificationService, ILogger<CreateJobCommandHandler> logger) : IRequestHandler<CreateJobCommand, int>
{
	public async Task<int> Handle(CreateJobCommand request, CancellationToken cancellationToken)
	{
		var userId = currentUserService.UserId;
		logger.LogInformation("🆕 Starting job creation process | UserId={UserId}, JobTitle={JobTitle}", userId, request.Title);

		var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);

		if (client == null)
		{
			logger.LogWarning("❌ Client not found during job creation | UserId={UserId}", userId);
			throw new NotFoundException(nameof(Client), currentUserService.UserId!);
		}

		logger.LogDebug("✅ Client found | ClientId={ClientId}, UserId={UserId}", client.Id, userId);

		var job = mapper.Map<Job>(request);
		job.ClientId = client.Id;

		if (request.CategoryIds is not null && request.CategoryIds.Any())
		{
			logger.LogDebug("🏷️ Processing categories | CategoryCount={CategoryCount}, CategoryIds={CategoryIds}", 
				request.CategoryIds.Count(), string.Join(",", request.CategoryIds));

			var categories = await unitOfWork.Categories.GetCategoriesByIdsAsync(request.CategoryIds);

			if (categories.Count != request.CategoryIds.Count())
			{
				var foundIds = categories.Select(c => c.Id);
				var missingIds = request.CategoryIds.Except(foundIds);
				logger.LogWarning("❌ Some categories not found | RequestedIds={RequestedIds}, FoundIds={FoundIds}, MissingIds={MissingIds}", 
					string.Join(",", request.CategoryIds), string.Join(",", foundIds), string.Join(",", missingIds));
				throw new MissingCategoriesException("Some selected categories could not be found.");
			}

			foreach (var category in categories)
			{
				job.Categories.Add(new JobCategory { CategoryId = category.Id });
			}

			logger.LogDebug("✅ Categories processed successfully | AddedCategoryCount={CategoryCount}", categories.Count);
		}

		if (request.SkillIds is not null && request.SkillIds.Any())
		{
			logger.LogDebug("🎯 Processing skills | SkillCount={SkillCount}, SkillIds={SkillIds}", 
				request.SkillIds.Count(), string.Join(",", request.SkillIds));

			var skills = await unitOfWork.Skills.GetSkillsByIdsAsync(request.SkillIds);

			if (skills.Count != request.SkillIds.Count())
			{
				var foundIds = skills.Select(s => s.Id);
				var missingIds = request.SkillIds.Except(foundIds);
				logger.LogWarning("❌ Some skills not found | RequestedIds={RequestedIds}, FoundIds={FoundIds}, MissingIds={MissingIds}", 
					string.Join(",", request.SkillIds), string.Join(",", foundIds), string.Join(",", missingIds));
				throw new MissingSkillsException("Some selected skills could not be found.");
			}

			foreach (var skill in skills)
			{
				job.Skills.Add(new JobSkill { SkillId = skill.Id });
			}

			logger.LogDebug("✅ Skills processed successfully | AddedSkillCount={SkillCount}", skills.Count);
		}

		logger.LogDebug("💾 Saving job to database | ClientId={ClientId}, Budget=${BudgetMin}-${BudgetMax}, Deadline={Deadline}", 
			job.ClientId, request.BudgetMin, request.BudgetMax, request.Deadline);

		await unitOfWork.Jobs.CreateAsync(job);
		await unitOfWork.SaveChangesAsync();

		logger.LogInformation("✅ Job created successfully | JobId={JobId}, ClientId={ClientId}, Title={JobTitle}", 
			job.Id, job.ClientId, request.Title);

		// Notify admins about new job submission
		try
		{
			logger.LogDebug("📨 Sending admin notification for job approval | JobId={JobId}", job.Id);
			await notificationService.NotifyJobSubmittedForApprovalAsync(job.Id);
			logger.LogDebug("✅ Admin notification sent successfully | JobId={JobId}", job.Id);
		}
		catch (Exception ex)
		{
			// Log error but don't fail the job creation
			logger.LogError(ex, "❌ Failed to send admin notification for job {JobId}", job.Id);
		}

		logger.LogInformation("🎉 Job creation process completed successfully | JobId={JobId}, UserId={UserId}", job.Id, userId);
		return job.Id;
	}
}
