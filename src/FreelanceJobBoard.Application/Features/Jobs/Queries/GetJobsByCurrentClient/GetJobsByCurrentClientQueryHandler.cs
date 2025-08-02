using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobsByCurrentClient;

internal class GetJobsByCurrentClientQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService) 
    : IRequestHandler<GetJobsByCurrentClientQuery, IEnumerable<JobDto>>
{
    public async Task<IEnumerable<JobDto>> Handle(GetJobsByCurrentClientQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to view their jobs");

        var client = await unitOfWork.Clients.GetByUserIdAsync(currentUserService.UserId!);
        if (client == null)
            throw new NotFoundException("Client", currentUserService.UserId!);

        var jobs = await unitOfWork.Jobs.GetJobsByClientIdAsync(client.Id);

        if (!string.IsNullOrEmpty(request.Status))
        {
            jobs = jobs.Where(j => j.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        }

        jobs = jobs.Skip((request.PageNumber - 1) * request.PageSize)
                  .Take(request.PageSize);

        return mapper.Map<IEnumerable<JobDto>>(jobs);
    }
}