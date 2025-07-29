using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;
public interface IProposalRepository : IGenericRepository<Proposal>
{
	Task<IEnumerable<Proposal>> GetAllByFreelancerIdAsync(int freelancerId);
	IQueryable<Proposal> GetByIdWithDetailsQueryable(int proposalId);
}
