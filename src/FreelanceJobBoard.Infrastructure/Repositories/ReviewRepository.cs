using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Review?> GetByJobIdAsync(int jobId)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.JobId == jobId);
    }

    public async Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.ReviewerId == reviewerId)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByRevieweeIdAsync(string revieweeId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.RevieweeId == revieweeId)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetVisibleReviewsByRevieweeIdAsync(string revieweeId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => r.RevieweeId == revieweeId && r.IsVisible && r.IsActive)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync();
    }

    public async Task<decimal> GetAverageRatingByRevieweeIdAsync(string revieweeId)
    {
        var reviews = await _dbSet
            .AsNoTracking()
            .Where(r => r.RevieweeId == revieweeId && r.IsVisible && r.IsActive)
            .ToListAsync();

        return reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;
    }

    public async Task<int> GetTotalReviewCountByRevieweeIdAsync(string revieweeId)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(r => r.RevieweeId == revieweeId && r.IsVisible && r.IsActive);
    }

    public async Task<bool> CanUserReviewJobAsync(int jobId, string userId)
    {
        var job = await _context.Jobs
            .AsNoTracking()
            .Include(j => j.Client)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null || job.Status != Domain.Constants.JobStatus.Completed)
            return false;

        var contract = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Proposal)
                .ThenInclude(p => p.Freelancer)
            .FirstOrDefaultAsync(c => c.Proposal.JobId == jobId && 
                                   c.ContractStatusId == 3); 

        if (contract == null)
            return false;

        var isClient = job.Client.UserId == userId;
        var isAcceptedFreelancer = contract.Proposal.Freelancer?.UserId == userId;

        return isClient || isAcceptedFreelancer;
    }

    public async Task<bool> HasUserReviewedJobAsync(int jobId, string reviewerId)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(r => r.JobId == jobId && r.ReviewerId == reviewerId);
    }
}