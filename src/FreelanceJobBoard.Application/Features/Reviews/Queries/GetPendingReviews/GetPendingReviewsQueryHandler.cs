using MediatR;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Features.Reviews.DTOs;
using FreelanceJobBoard.Domain.Constants;
using AutoMapper;

namespace FreelanceJobBoard.Application.Features.Reviews.Queries.GetPendingReviews;

public class GetPendingReviewsQueryHandler : IRequestHandler<GetPendingReviewsQuery, PendingReviewsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPendingReviewsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PendingReviewsDto> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        var result = new PendingReviewsDto
        {
            UserId = request.UserId,
            PendingReviews = new List<PendingReviewItemDto>()
        };

        var client = await _unitOfWork.Clients.GetByUserIdAsync(request.UserId);
        var freelancer = await _unitOfWork.Freelancers.GetByUserIdAsync(request.UserId);

        var pendingReviewJobs = new List<(FreelanceJobBoard.Domain.Entities.Job job, string reviewType, string revieweeName, string revieweeId)>();

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
                    var acceptedProposal = job.Proposals?.FirstOrDefault(p => p.Status == ProposalStatus.Accepted);
                    if (acceptedProposal?.Freelancer?.User != null)
                    {
                        pendingReviewJobs.Add((
                            job, 
                            ReviewType.ClientToFreelancer,
                            acceptedProposal.Freelancer.User.FullName ?? "Freelancer",
                            acceptedProposal.Freelancer.UserId ?? ""
                        ));
                    }
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
                        if (job.Client?.User != null)
                        {
                            pendingReviewJobs.Add((
                                job,
                                ReviewType.FreelancerToClient,
                                job.Client.User.FullName ?? "Client",
                                job.Client.UserId ?? ""
                            ));
                        }
                    }
                }
            }
        }

        // Convert to DTOs - no need for Distinct() since we're handling each role separately
        foreach (var (job, reviewType, revieweeName, revieweeId) in pendingReviewJobs)
        {
            if (!string.IsNullOrEmpty(revieweeId))
            {
                result.PendingReviews.Add(new PendingReviewItemDto
                {
                    JobId = job.Id,
                    JobTitle = job.Title ?? "Unknown Job",
                    ReviewType = reviewType,
                    RevieweeName = revieweeName,
                    RevieweeId = revieweeId,
                    CompletedDate = job.CompletedDate,
                    IsUrgent = job.CompletedDate.HasValue && 
                              DateTime.UtcNow.Subtract(job.CompletedDate.Value).TotalDays > 7
                });
            }
        }

        result.TotalPending = result.PendingReviews.Count;
        return result;
    }
}