using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

public class ContractRepository : GenericRepository<Contract>, IContractRepository
{
    private readonly ILogger<ContractRepository>? _logger;

    public ContractRepository(ApplicationDbContext context) : base(context)
    {
        _logger = null; // Logger is optional for now to maintain compatibility
    }

    public ContractRepository(ApplicationDbContext context, ILogger<ContractRepository> logger) : base(context)
    {
        _logger = logger;
    }

    public async Task<Contract?> GetContractByProposalIdAsync(int proposalId)
    {
        try
        {
            return await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl.User)
                .Include(c => c.Freelancer)
                    .ThenInclude(f => f.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p.Job)
                .FirstOrDefaultAsync(c => c.ProposalId == proposalId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting contract by proposal ID {ProposalId}", proposalId);
            throw;
        }
    }

    public async Task<IEnumerable<Contract>> GetContractsByClientIdAsync(int clientId)
    {
        try
        {
            return await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Freelancer)
                    .ThenInclude(f => f.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p.Job)
                .Where(c => c.ClientId == clientId && c.IsActive)
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting contracts by client ID {ClientId}", clientId);
            throw;
        }
    }

    public async Task<IEnumerable<Contract>> GetContractsByFreelancerIdAsync(int freelancerId)
    {
        try
        {
            return await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p.Job)
                .Where(c => c.FreelancerId == freelancerId && c.IsActive)
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting contracts by freelancer ID {FreelancerId}", freelancerId);
            throw;
        }
    }

    public async Task<Contract?> GetContractWithDetailsAsync(int contractId)
    {
        try
        {
            _logger?.LogInformation("Getting contract with details for contract ID {ContractId}", contractId);
            
            var contract = await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl != null ? cl.User : null)
                .Include(c => c.Freelancer)
                    .ThenInclude(f => f != null ? f.User : null)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p != null ? p.Job : null)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.IsActive);

            if (contract != null)
            {
                _logger?.LogInformation("Contract {ContractId} found successfully", contractId);
            }
            else
            {
                _logger?.LogWarning("Contract {ContractId} not found or inactive", contractId);
            }

            return contract;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting contract with details for contract ID {ContractId}", contractId);
            throw;
        }
    }

    public async Task<IEnumerable<Contract>> GetActiveContractsAsync()
    {
        try
        {
            return await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl.User)
                .Include(c => c.Freelancer)
                    .ThenInclude(f => f.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p.Job)
                .Where(c => c.ContractStatus.Name == Domain.Constants.ContractStatus.Active && c.IsActive)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting active contracts");
            throw;
        }
    }
}