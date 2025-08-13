using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;
public interface IProposalRepository : IGenericRepository<Proposal>
{
	Task<IEnumerable<Proposal>> GetAllByFreelancerIdAsync(int freelancerId);
	Task<IEnumerable<Proposal>> GetProposalsByJobIdAsync(int jobId);
	Task<Proposal?> GetProposalWithDetailsAsync(int proposalId);
	IQueryable<Proposal> GetByIdWithDetailsQueryable(int proposalId);
	Task<IEnumerable<ProposalsPerDayResultDto>> GetNumOfProposalsPerDayAsync();
}
