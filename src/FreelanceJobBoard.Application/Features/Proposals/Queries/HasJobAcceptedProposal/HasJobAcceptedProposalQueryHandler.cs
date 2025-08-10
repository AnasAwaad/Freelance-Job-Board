using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Constants;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.HasJobAcceptedProposal;

public class HasJobAcceptedProposalQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<HasJobAcceptedProposalQuery, bool>
{
    public async Task<bool> Handle(HasJobAcceptedProposalQuery request, CancellationToken cancellationToken)
    {
        var allProposals = await unitOfWork.Proposals.GetAllAsync();
        
        return allProposals.Any(p => p.JobId == request.JobId && p.Status == ProposalStatus.Accepted);
    }
}