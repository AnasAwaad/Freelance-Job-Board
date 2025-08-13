using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class ProposalRepository : GenericRepository<Proposal>, IProposalRepository
{
	public ProposalRepository(ApplicationDbContext context, ILogger<GenericRepository<Proposal>>? logger = null) : base(context, logger)
	{
	}

	public async Task<IEnumerable<Proposal>> GetProposalsByJobIdAsync(int jobId)
	{
		return await _context.Proposals
			.Include(p => p.Freelancer)
				.ThenInclude(f => f!.User)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.Where(p => p.JobId == jobId)
			.OrderByDescending(p => p.CreatedOn)
			.ToListAsync();
	}

	public async Task<IEnumerable<Proposal>> GetProposalsByFreelancerIdAsync(int freelancerId)
	{
		return await _context.Proposals
			.Include(p => p.Job)
				.ThenInclude(j => j!.Client)
					.ThenInclude(c => c!.User)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.Where(p => p.FreelancerId == freelancerId)
			.OrderByDescending(p => p.CreatedOn)
			.ToListAsync();
	}

	public async Task<Proposal?> GetProposalWithDetailsAsync(int proposalId)
	{
		return await _context.Proposals
			.Include(p => p.Freelancer)
				.ThenInclude(f => f!.User)
			.Include(p => p.Job)
				.ThenInclude(j => j!.Client)
					.ThenInclude(c => c!.User)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.FirstOrDefaultAsync(p => p.Id == proposalId);
	}

	public async Task<Proposal?> GetProposalByFreelancerAndJobAsync(int freelancerId, int jobId)
	{
		return await _context.Proposals
			.Include(p => p.Freelancer)
				.ThenInclude(f => f!.User)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.FirstOrDefaultAsync(p => p.FreelancerId == freelancerId && p.JobId == jobId);
	}

	public async Task<int> GetProposalCountByJobIdAsync(int jobId)
	{
		return await _context.Proposals
			.CountAsync(p => p.JobId == jobId);
	}

	public async Task<Proposal?> GetAcceptedProposalByJobIdAsync(int jobId)
	{
		return await _context.Proposals
			.Include(p => p.Freelancer)
				.ThenInclude(f => f!.User)
			.FirstOrDefaultAsync(p => p.JobId == jobId && p.Status == Domain.Constants.ProposalStatus.Accepted);
	}

	public async Task<IEnumerable<Proposal>> GetAllByFreelancerIdAsync(int freelancerId)
	{
		return await _context.Proposals
			.Include(p => p.Job)
				.ThenInclude(j => j!.Client)
					.ThenInclude(c => c!.User)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.Where(p => p.FreelancerId == freelancerId)
			.OrderByDescending(p => p.CreatedOn)
			.ToListAsync();
	}

	public IQueryable<Proposal> GetByIdWithDetailsQueryable(int proposalId)
	{
		return _context.Proposals
			.Include(p => p.Freelancer)
				.ThenInclude(f => f!.User)
			.Include(p => p.Job)
				.ThenInclude(j => j!.Client)
					.ThenInclude(c => c!.User)
			.Include(p => p.Attachments)
				.ThenInclude(a => a.Attachment)
			.Where(p => p.Id == proposalId);
	}
}
