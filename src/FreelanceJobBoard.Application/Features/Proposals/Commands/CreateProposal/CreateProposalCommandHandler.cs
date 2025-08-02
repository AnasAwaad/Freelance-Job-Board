using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
internal class CreateProposalCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService, ICurrentUserService currentUserService, INotificationService notificationService, ILogger<CreateProposalCommandHandler> logger) : IRequestHandler<CreateProposalCommand>
{
	public async Task Handle(CreateProposalCommand request, CancellationToken cancellationToken)
	{
		if (!currentUserService.IsAuthenticated)
			throw new UnauthorizedAccessException("User must be authenticated to submit a proposal");

		var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
		if (freelancer == null)
			throw new NotFoundException("Freelancer", currentUserService.UserId!);

		var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId);
		if (job is null)
			throw new NotFoundException(nameof(Job), request.JobId.ToString());

		if (job.Status != JobStatus.Open)
			throw new InvalidOperationException("This job is no longer accepting proposals");

		var jobProposals = await unitOfWork.Proposals.GetProposalsByJobIdAsync(request.JobId);
		var hasExistingProposal = jobProposals.Any(p => p.FreelancerId == freelancer.Id);
		
		if (hasExistingProposal)
			throw new InvalidOperationException("You have already submitted a proposal for this job");

		var proposal = mapper.Map<Proposal>(request);
		proposal.FreelancerId = freelancer.Id;
		proposal.ClientId = job.ClientId;
		proposal.Status = ProposalStatus.Submitted;

		if (request.PortfolioFiles is not null && request.PortfolioFiles.Count > 0)
		{
			proposal.Attachments = new List<ProposalAttachment>();

			foreach (var file in request.PortfolioFiles)
			{
				var fileUrl = await cloudinaryService.UploadFileAsync(file, "proposals");

				var attachment = new Attachment
				{
					FileName = file.FileName,
					FileSize = file.Length,
					FilePath = fileUrl,
					FileType = file.ContentType
				};

				proposal.Attachments.Add(new ProposalAttachment { Attachment = attachment });
			}
		}

		await unitOfWork.Proposals.CreateAsync(proposal);
		await unitOfWork.SaveChangesAsync();

		try
		{
			await notificationService.NotifyNewProposalAsync(request.JobId, proposal.Id);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to send new proposal notification for job {JobId}", request.JobId);
		}
	}
}
