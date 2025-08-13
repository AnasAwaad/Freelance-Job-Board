using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

public class ContractRepository : GenericRepository<Contract>, IContractRepository
{
    public ContractRepository(ApplicationDbContext context, ILogger<GenericRepository<Contract>>? logger = null) : base(context, logger)
    {
    }

    public async Task<Contract?> GetContractByProposalIdAsync(int proposalId)
    {
        try
        {
            _logger?.LogDebug("?? Getting contract by proposal ID | ProposalId={ProposalId}", proposalId);
            
            var contract = await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl!.User)
                .Include(c => c.Freelancer)
                    .ThenInclude(f => f!.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p!.Job)
                .FirstOrDefaultAsync(c => c.ProposalId == proposalId);

            if (contract != null)
            {
                _logger?.LogDebug("? Contract found by proposal ID | ProposalId={ProposalId}, ContractId={ContractId}", proposalId, contract.Id);
            }
            else
            {
                _logger?.LogDebug("? Contract not found by proposal ID | ProposalId={ProposalId}", proposalId);
            }

            return contract;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error getting contract by proposal ID | ProposalId={ProposalId}", proposalId);
            throw;
        }
    }

    public async Task<IEnumerable<Contract>> GetContractsByClientIdAsync(int clientId)
    {
        try
        {
            _logger?.LogDebug("?? Getting contracts by client ID | ClientId={ClientId}", clientId);
            
            var contracts = await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Freelancer)
                    .ThenInclude(f => f!.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p!.Job)
                .Where(c => c.ClientId == clientId && c.IsActive)
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();

            _logger?.LogDebug("? Contracts retrieved by client ID | ClientId={ClientId}, Count={Count}", clientId, contracts.Count());
            return contracts;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error getting contracts by client ID | ClientId={ClientId}", clientId);
            throw;
        }
    }

    public async Task<IEnumerable<Contract>> GetContractsByFreelancerIdAsync(int freelancerId)
    {
        try
        {
            _logger?.LogDebug("?? Getting contracts by freelancer ID | FreelancerId={FreelancerId}", freelancerId);
            
            var contracts = await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl!.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p!.Job)
                .Where(c => c.FreelancerId == freelancerId && c.IsActive)
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();

            _logger?.LogDebug("? Contracts retrieved by freelancer ID | FreelancerId={FreelancerId}, Count={Count}", freelancerId, contracts.Count());
            return contracts;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error getting contracts by freelancer ID | FreelancerId={FreelancerId}", freelancerId);
            throw;
        }
    }

    public async Task<Contract?> GetContractWithDetailsAsync(int contractId)
    {
        try
        {
            _logger?.LogDebug("?? Getting contract with details | ContractId={ContractId}", contractId);
            
            var contract = await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl!.User)
                .Include(c => c.Freelancer)
                    .ThenInclude(f => f!.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p!.Job)
                .FirstOrDefaultAsync(c => c.Id == contractId && c.IsActive);

            if (contract != null)
            {
                _logger?.LogDebug("? Contract with details found | ContractId={ContractId}, Status={Status}", contractId, contract.ContractStatus?.Name);
            }
            else
            {
                _logger?.LogDebug("? Contract with details not found | ContractId={ContractId}", contractId);
            }

            return contract;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error getting contract with details | ContractId={ContractId}", contractId);
            throw;
        }
    }

    public async Task<IEnumerable<Contract>> GetActiveContractsAsync()
    {
        try
        {
            _logger?.LogDebug("?? Getting all active contracts");
            
            var contracts = await _context.Contracts
                .Include(c => c.ContractStatus)
                .Include(c => c.Client)
                    .ThenInclude(cl => cl!.User)
                .Include(c => c.Freelancer)
                    .ThenInclude(f => f!.User)
                .Include(c => c.Proposal)
                    .ThenInclude(p => p!.Job)
                .Where(c => c.ContractStatus!.Name == Domain.Constants.ContractStatus.Active && c.IsActive)
                .ToListAsync();

            _logger?.LogDebug("? Active contracts retrieved | Count={Count}", contracts.Count());
            return contracts;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error getting active contracts");
            throw;
        }
    }
}