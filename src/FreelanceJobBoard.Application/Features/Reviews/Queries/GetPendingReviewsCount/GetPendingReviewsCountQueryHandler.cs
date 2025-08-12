using MediatR;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetPendingReviewsCount;

public class GetPendingReviewsCountQueryHandler : IRequestHandler<GetPendingReviewsCountQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingReviewsCountQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(GetPendingReviewsCountQuery request, CancellationToken cancellationToken)
    {
        var count = 0;

        var client = await _unitOfWork.Clients.GetByUserIdAsync(request.UserId);
        var freelancer = await _unitOfWork.Freelancers.GetByUserIdAsync(request.UserId);

        if (client != null)
        {
            var clientJobs = await _unitOfWork.Jobs.GetJobsByClientIdAsync(client.Id);
            var completedClientJobs = clientJobs.Where(j => j.Status == JobStatus.Completed);
            
            foreach (var job in completedClientJobs)
            {
                var canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(job.Id, request.UserId);
                var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedJobAsync(job.Id, request.UserId);

                if (canReview && !hasReviewed)
                {
                    count++;
                }
            }
        }

        if (freelancer != null)
        {
            var freelancerProposals = await _unitOfWork.Proposals.GetAllByFreelancerIdAsync(freelancer.Id);
            var acceptedProposals = freelancerProposals.Where(p => p.Status == ProposalStatus.Accepted);
            
            foreach (var proposal in acceptedProposals)
            {
                var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(proposal.JobId);
                if (job != null && job.Status == JobStatus.Completed)
                {
                    var canReview = await _unitOfWork.Reviews.CanUserReviewJobAsync(job.Id, request.UserId);
                    var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedJobAsync(job.Id, request.UserId);

                    if (canReview && !hasReviewed)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }
}