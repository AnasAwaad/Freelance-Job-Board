using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
internal class CreateProposalCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateProposalCommand>
{
	public async Task Handle(CreateProposalCommand request, CancellationToken cancellationToken)
	{
		var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId);

		if (job is null)
			throw new NotFoundException(nameof(Job), request.JobId.ToString());

		var proposal = mapper.Map<Proposal>(request);

		// TODO : Add client Id 
		// proposal.ClientId = job.ClientId.Value;


		//TODO : upload attachement 


		await unitOfWork.Proposals.CreateAsync(proposal);
		await unitOfWork.SaveChangesAsync();

	}
}
