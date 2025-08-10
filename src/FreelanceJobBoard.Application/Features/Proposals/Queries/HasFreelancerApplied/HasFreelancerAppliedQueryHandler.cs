using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.HasFreelancerApplied;

public class HasFreelancerAppliedQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<HasFreelancerAppliedQuery, bool>
{
    public async Task<bool> Handle(HasFreelancerAppliedQuery request, CancellationToken cancellationToken)
    {
        var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(request.UserId);
        if (freelancer == null)
            return false;

        var allProposals = await unitOfWork.Proposals.GetAllAsync();
        
        return allProposals.Any(p => p.JobId == request.JobId && p.FreelancerId == freelancer.Id);
    }
}