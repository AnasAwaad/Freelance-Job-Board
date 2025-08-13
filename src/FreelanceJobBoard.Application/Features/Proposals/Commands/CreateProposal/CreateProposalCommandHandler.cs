using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
public class CreateProposalCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService, ICurrentUserService currentUserService, INotificationService notificationService, ILogger<CreateProposalCommandHandler> logger) : IRequestHandler<CreateProposalCommand>
{
	public async Task Handle(CreateProposalCommand request, CancellationToken cancellationToken)
	{
		var userId = currentUserService.UserId;
		logger.LogInformation("🆕 Starting proposal creation | JobId={JobId}, UserId={UserId}, BidAmount=${BidAmount}", 
			request.JobId, userId, request.BidAmount);

		if (!currentUserService.IsAuthenticated)
		{
			logger.LogWarning("❌ Unauthenticated proposal attempt | JobId={JobId}", request.JobId);
			throw new UnauthorizedAccessException("User must be authenticated to submit a proposal");
		}

		var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
		if (freelancer == null)
		{
			logger.LogWarning("❌ Freelancer not found during proposal creation | UserId={UserId}, JobId={JobId}", 
				userId, request.JobId);
			throw new NotFoundException("Freelancer", currentUserService.UserId!);
		}

		logger.LogDebug("✅ Freelancer found | FreelancerId={FreelancerId}, UserId={UserId}", 
			freelancer.Id, userId);

		var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId);
		if (job is null)
		{
			logger.LogWarning("❌ Job not found for proposal | JobId={JobId}, FreelancerId={FreelancerId}", 
				request.JobId, freelancer.Id);
			throw new NotFoundException(nameof(Job), request.JobId.ToString());
		}

		logger.LogDebug("✅ Job found | JobId={JobId}, JobStatus={JobStatus}, ClientId={ClientId}", 
			job.Id, job.Status, job.ClientId);

		if (job.Status != JobStatus.Open)
		{
			logger.LogWarning("❌ Job not accepting proposals | JobId={JobId}, JobStatus={JobStatus}, FreelancerId={FreelancerId}", 
				request.JobId, job.Status, freelancer.Id);
			throw new InvalidOperationException("This job is no longer accepting proposals");
		}

		// Check if job already has an accepted proposal
		var jobProposals = await unitOfWork.Proposals.GetProposalsByJobIdAsync(request.JobId);
		var hasAcceptedProposal = jobProposals.Any(p => p.Status == ProposalStatus.Accepted);
		
		if (hasAcceptedProposal)
		{
			logger.LogWarning("❌ Job already has accepted proposal | JobId={JobId}, FreelancerId={FreelancerId}", 
				request.JobId, freelancer.Id);
			throw new InvalidOperationException("This job has already been assigned to another freelancer");
		}

		var hasExistingProposal = jobProposals.Any(p => p.FreelancerId == freelancer.Id);
		
		if (hasExistingProposal)
		{
			logger.LogWarning("❌ Duplicate proposal attempt | JobId={JobId}, FreelancerId={FreelancerId}", 
				request.JobId, freelancer.Id);
			throw new InvalidOperationException("You have already submitted a proposal for this job");
		}

		logger.LogDebug("✅ Proposal validation passed | JobId={JobId}, FreelancerId={FreelancerId}, ExistingProposalCount={ProposalCount}", 
			request.JobId, freelancer.Id, jobProposals.Count());

		var proposal = mapper.Map<Proposal>(request);
		proposal.FreelancerId = freelancer.Id;
		proposal.ClientId = job.ClientId;
		proposal.Status = ProposalStatus.Submitted;

		logger.LogDebug("📄 Proposal mapped | JobId={JobId}, FreelancerId={FreelancerId}, BidAmount=${BidAmount}, TimelineDays={TimelineDays}", 
			request.JobId, freelancer.Id, request.BidAmount, request.EstimatedTimelineDays);

		if (request.PortfolioFiles is not null && request.PortfolioFiles.Count > 0)
		{
			logger.LogDebug("📎 Processing portfolio files | JobId={JobId}, FileCount={FileCount}", 
				request.JobId, request.PortfolioFiles.Count);

			proposal.Attachments = new List<ProposalAttachment>();

			foreach (var file in request.PortfolioFiles)
			{
				try
				{
					logger.LogDebug("☁️ Uploading file to Cloudinary | FileName={FileName}, FileSize={FileSize}", 
						file.FileName, file.Length);

					var fileUrl = await cloudinaryService.UploadFileAsync(file, "proposals");

					var attachment = new Attachment
					{
						FileName = file.FileName,
						FileSize = file.Length,
						FilePath = fileUrl,
						FileType = file.ContentType
					};

					proposal.Attachments.Add(new ProposalAttachment { Attachment = attachment });

					logger.LogDebug("✅ File uploaded successfully | FileName={FileName}, FileUrl={FileUrl}", 
						file.FileName, fileUrl);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "❌ Failed to upload file | FileName={FileName}, JobId={JobId}", 
						file.FileName, request.JobId);
					throw new InvalidOperationException($"Failed to upload file: {file.FileName}");
				}
			}

			logger.LogDebug("✅ All portfolio files processed | JobId={JobId}, AttachmentCount={AttachmentCount}", 
				request.JobId, proposal.Attachments.Count);
		}

		logger.LogDebug("💾 Saving proposal to database | JobId={JobId}, FreelancerId={FreelancerId}", 
			request.JobId, freelancer.Id);

		await unitOfWork.Proposals.CreateAsync(proposal);
		await unitOfWork.SaveChangesAsync();

		logger.LogInformation("✅ Proposal created successfully | ProposalId={ProposalId}, JobId={JobId}, FreelancerId={FreelancerId}, BidAmount=${BidAmount}", 
			proposal.Id, request.JobId, freelancer.Id, request.BidAmount);

		try
		{
			logger.LogDebug("📨 Sending new proposal notification | JobId={JobId}, ProposalId={ProposalId}", 
				request.JobId, proposal.Id);
			
			await notificationService.NotifyNewProposalAsync(request.JobId, proposal.Id);
			
			logger.LogDebug("✅ New proposal notification sent successfully | ProposalId={ProposalId}", proposal.Id);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "❌ Failed to send new proposal notification | JobId={JobId}, ProposalId={ProposalId}", 
				request.JobId, proposal.Id);
		}

		logger.LogInformation("🎉 Proposal creation process completed | ProposalId={ProposalId}, JobId={JobId}, FreelancerId={FreelancerId}", 
			proposal.Id, request.JobId, freelancer.Id);
	}
}
