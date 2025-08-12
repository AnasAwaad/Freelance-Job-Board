using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobsByCurrentFreelancer;

internal class GetJobsByCurrentFreelancerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService) 
    : IRequestHandler<GetJobsByCurrentFreelancerQuery, IEnumerable<JobDto>>
{
    public async Task<IEnumerable<JobDto>> Handle(GetJobsByCurrentFreelancerQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to view their jobs");

        var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(currentUserService.UserId!);
        if (freelancer == null)
            throw new NotFoundException("Freelancer", currentUserService.UserId!);

        // Get jobs where the freelancer has an accepted proposal
        var jobs = await unitOfWork.Jobs.GetJobsByFreelancerIdAsync(freelancer.Id);

        if (!string.IsNullOrEmpty(request.Status))
        {
            jobs = jobs.Where(j => j.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        }

        jobs = jobs.Skip((request.PageNumber - 1) * request.PageSize)
                  .Take(request.PageSize);

        return mapper.Map<IEnumerable<JobDto>>(jobs);
    }
}