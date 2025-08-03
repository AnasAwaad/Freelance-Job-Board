using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<Review?> GetByJobIdAsync(int jobId);
    Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId);
    Task<IEnumerable<Review>> GetByRevieweeIdAsync(string revieweeId);
    Task<IEnumerable<Review>> GetVisibleReviewsByRevieweeIdAsync(string revieweeId);
    Task<decimal> GetAverageRatingByRevieweeIdAsync(string revieweeId);
    Task<int> GetTotalReviewCountByRevieweeIdAsync(string revieweeId);
    Task<bool> CanUserReviewJobAsync(int jobId, string userId);
    Task<bool> HasUserReviewedJobAsync(int jobId, string reviewerId);
}