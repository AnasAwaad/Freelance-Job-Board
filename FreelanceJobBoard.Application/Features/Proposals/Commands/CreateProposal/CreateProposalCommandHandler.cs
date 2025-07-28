using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
internal class CreateProposalCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService) : IRequestHandler<CreateProposalCommand>
{
	public async Task Handle(CreateProposalCommand request, CancellationToken cancellationToken)
	{

		var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(request.UserId);

		if (freelancer is null)
			throw new UnauthorizedAccessException();

		var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId);

		if (job is null)
			throw new NotFoundException(nameof(Job), request.JobId.ToString());

		var proposal = mapper.Map<Proposal>(request);

		proposal.ClientId = job.ClientId;
		proposal.FreelancerId = freelancer.Id;


		// upload attachement 
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

	}
}
