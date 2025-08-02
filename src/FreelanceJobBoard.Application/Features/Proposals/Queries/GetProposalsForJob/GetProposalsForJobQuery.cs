using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Proposals.Queries.GetProposalsForJob;

public class GetProposalsForJobQuery : IRequest<IEnumerable<ProposalDto>>
{
    public int JobId { get; set; }
    public string? Status { get; set; }
}