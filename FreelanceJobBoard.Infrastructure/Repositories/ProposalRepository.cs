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
			.Where(p => p.FreelancerId == freelancerId)
			.Include(p => p.Job)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.Include(p => p.Freelancer)
				.ThenInclude(f => f!.User)
			.OrderByDescending(p => p.CreatedOn)
			.ToListAsync();
	}

	public async Task<IEnumerable<Proposal>> GetProposalsByJobIdAsync(int jobId)
	{
		return await _context.Proposals
			.Where(p => p.JobId == jobId)
			.Include(p => p.Freelancer)
				.ThenInclude(f => f!.User)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.OrderByDescending(p => p.CreatedOn)
			.ToListAsync();
	}

	public async Task<Proposal?> GetProposalWithDetailsAsync(int proposalId)
	{
		return await _context.Proposals
			.Include(p => p.Job)
			.Include(p => p.Freelancer)
				.ThenInclude(f => f!.User)
			.Include(p => p.Client)
				.ThenInclude(c => c!.User)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.FirstOrDefaultAsync(p => p.Id == proposalId);
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
