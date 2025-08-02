using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Commands.DeleteFreelancerProposal;
internal class DeleteProposalForFreelancerCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteProposalForFreelancerCommand>
{
	public async Task Handle(DeleteProposalForFreelancerCommand request, CancellationToken cancellationToken)
	{
		var proposal = await unitOfWork.Proposals.GetByIdAsync(request.ProposalId);

		if (proposal is null)
			throw new NotFoundException(nameof(Proposal), request.ProposalId.ToString());

		unitOfWork.Proposals.Delete(proposal);

		//TODO : Delete attachments related to proposal in cloudinary

		await unitOfWork.SaveChangesAsync();
	}
}
