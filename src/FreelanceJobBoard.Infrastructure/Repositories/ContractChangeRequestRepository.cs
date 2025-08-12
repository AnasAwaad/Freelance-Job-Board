using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

public class ContractChangeRequestRepository : GenericRepository<ContractChangeRequest>, IContractChangeRequestRepository
{
    public ContractChangeRequestRepository(ApplicationDbContext context, ILogger<GenericRepository<ContractChangeRequest>>? logger = null) : base(context, logger)
    {
    }

    public async Task<IEnumerable<ContractChangeRequest>> GetPendingRequestsAsync(int contractId)
    {
        return await _context.ContractChangeRequests
            .Include(r => r.FromVersion)
            .Include(r => r.ProposedVersion)
            .Where(r => r.ContractId == contractId && 
                       r.Status == Domain.Constants.ContractChangeRequestStatus.Pending &&
                       r.IsActive)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContractChangeRequest>> GetRequestHistoryAsync(int contractId)
    {
        return await _context.ContractChangeRequests
            .Include(r => r.FromVersion)
            .Include(r => r.ProposedVersion)
            .Where(r => r.ContractId == contractId && r.IsActive)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContractChangeRequest>> GetUserPendingRequestsAsync(string userId)
    {
        return await _context.ContractChangeRequests
            .Include(r => r.Contract)
                .ThenInclude(c => c!.Proposal)
                    .ThenInclude(p => p!.Job)
            .Include(r => r.Contract)
                .ThenInclude(c => c!.Client)
                    .ThenInclude(cl => cl!.User)
            .Include(r => r.Contract)
                .ThenInclude(c => c!.Freelancer)
                    .ThenInclude(f => f!.User)
            .Include(r => r.FromVersion)
            .Include(r => r.ProposedVersion)
            .Where(r => (r.RequestedByUserId == userId || 
                        (r.ResponseByUserId == null && r.RequestedByUserId != userId)) &&
                       r.Status == Domain.Constants.ContractChangeRequestStatus.Pending &&
                       r.IsActive)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();
    }

    public async Task<ContractChangeRequest?> GetRequestWithDetailsAsync(int requestId)
    {
        return await _context.ContractChangeRequests
            .Include(r => r.Contract)
                .ThenInclude(c => c!.Proposal)
                    .ThenInclude(p => p!.Job)
            .Include(r => r.Contract)
                .ThenInclude(c => c!.Client)
                    .ThenInclude(cl => cl!.User)
            .Include(r => r.Contract)
                .ThenInclude(c => c!.Freelancer)
                    .ThenInclude(f => f!.User)
            .Include(r => r.FromVersion)
            .Include(r => r.ProposedVersion)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.IsActive);
    }

    public async Task<bool> HasPendingChangesAsync(int contractId)
    {
        return await _context.ContractChangeRequests
            .AnyAsync(r => r.ContractId == contractId && 
                          r.Status == Domain.Constants.ContractChangeRequestStatus.Pending &&
                          r.IsActive);
    }
}