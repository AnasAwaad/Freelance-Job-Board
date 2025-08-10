using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.HasFreelancerApplied;

public record HasFreelancerAppliedQuery(int JobId, string UserId) : IRequest<bool>;