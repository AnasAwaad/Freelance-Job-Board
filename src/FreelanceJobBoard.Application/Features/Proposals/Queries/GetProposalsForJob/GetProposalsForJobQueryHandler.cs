using AutoMapper;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalsForJob;

internal class GetProposalsForJobQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService) 
    : IRequestHandler<GetProposalsForJobQuery, IEnumerable<ProposalDto>>
{
    public async Task<IEnumerable<ProposalDto>> Handle(GetProposalsForJobQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to view job proposals");

        var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
        if (client == null)
            throw new NotFoundException("Client", currentUserService.UserId!);

        var job = await unitOfWork.Jobs.GetByIdAsync(request.JobId);
        if (job == null)
            throw new NotFoundException(nameof(Job), request.JobId.ToString());

        if (job.ClientId != client.Id)
            throw new UnauthorizedAccessException("Only the job owner can view proposals for this job");

        var jobProposals = await unitOfWork.Proposals.GetProposalsByJobIdAsync(request.JobId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            jobProposals = jobProposals.Where(p => p.Status != null && p.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        }

        return mapper.Map<IEnumerable<ProposalDto>>(jobProposals);
    }
}