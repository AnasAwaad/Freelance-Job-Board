using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.HasJobAcceptedProposal;

public record HasJobAcceptedProposalQuery(int JobId) : IRequest<bool>;