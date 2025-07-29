using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class ProposalRepository : GenericRepository<Proposal>, IProposalRepository
{
	public ProposalRepository(ApplicationDbContext context) : base(context)
	{
	}

	public async Task<IEnumerable<Proposal>> GetAllByFreelancerIdAsync(int freelancerId)
	{
		return await _context.Proposals
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.ToListAsync();
	}

	public IQueryable<Proposal> GetByIdWithDetailsQueryable(int proposalId)
	{
		return _context.Proposals
			.Include(p => p.Client)
				.ThenInclude(c => c.User)
			.Include(p => p.Job)
			.Where(p => p.Id == proposalId);
	}
}
