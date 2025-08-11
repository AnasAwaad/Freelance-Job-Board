using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

public class ContractVersionRepository : GenericRepository<ContractVersion>, IContractVersionRepository
{
    private readonly ILogger<ContractVersionRepository>? _logger;

    public ContractVersionRepository(ApplicationDbContext context) : base(context)
    {
        _logger = null; // Logger is optional for backward compatibility
    }

    public ContractVersionRepository(ApplicationDbContext context, ILogger<ContractVersionRepository> logger) : base(context)
    {
        _logger = logger;
    }

    public async Task<ContractVersion?> GetCurrentVersionAsync(int contractId)
    {
        try
        {
            _logger?.LogInformation("Getting current version for contract {ContractId}", contractId);
            
            var currentVersion = await _context.ContractVersions
                .Where(v => v.ContractId == contractId && v.IsCurrentVersion && v.IsActive)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();

            if (currentVersion != null)
            {
                _logger?.LogInformation("Found current version {VersionId} (v{VersionNumber}) for contract {ContractId}", 
                    currentVersion.Id, currentVersion.VersionNumber, contractId);
            }
            else
            {
                _logger?.LogWarning("No current version found for contract {ContractId}", contractId);
            }

            return currentVersion;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting current version for contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<IEnumerable<ContractVersion>> GetVersionHistoryAsync(int contractId)
    {
        try
        {
            _logger?.LogInformation("Getting version history for contract {ContractId}", contractId);
            
            var versions = await _context.ContractVersions
                .Where(v => v.ContractId == contractId && v.IsActive)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            _logger?.LogInformation("Found {VersionCount} versions for contract {ContractId}", 
                versions.Count, contractId);

            return versions;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting version history for contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<ContractVersion?> GetVersionByNumberAsync(int contractId, int versionNumber)
    {
        try
        {
            return await _context.ContractVersions
                .FirstOrDefaultAsync(v => v.ContractId == contractId && 
                                         v.VersionNumber == versionNumber && 
                                         v.IsActive);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting version {VersionNumber} for contract {ContractId}", 
                versionNumber, contractId);
            throw;
        }
    }

    public async Task<int> GetNextVersionNumberAsync(int contractId)
    {
        try
        {
            var maxVersion = await _context.ContractVersions
                .Where(v => v.ContractId == contractId)
                .MaxAsync(v => (int?)v.VersionNumber) ?? 0;
            
            var nextVersion = maxVersion + 1;
            _logger?.LogInformation("Next version number for contract {ContractId} will be {VersionNumber}", 
                contractId, nextVersion);
            
            return nextVersion;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting next version number for contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<bool> UpdateCurrentVersionAsync(int contractId, int newCurrentVersionId)
    {
        try
        {
            _logger?.LogInformation("Updating current version for contract {ContractId} to version {VersionId}", 
                contractId, newCurrentVersionId);

            // First, set all versions to not current
            var allVersions = await _context.ContractVersions
                .Where(v => v.ContractId == contractId)
                .ToListAsync();
            
            foreach (var version in allVersions)
            {
                version.IsCurrentVersion = false;
                version.LastUpdatedOn = DateTime.UtcNow;
            }
            
            // Then set the new current version
            var newCurrentVersion = await _context.ContractVersions
                .FirstOrDefaultAsync(v => v.Id == newCurrentVersionId && v.ContractId == contractId);
            
            if (newCurrentVersion != null)
            {
                newCurrentVersion.IsCurrentVersion = true;
                newCurrentVersion.LastUpdatedOn = DateTime.UtcNow;
                
                _logger?.LogInformation("Successfully updated current version for contract {ContractId} to version {VersionId} (v{VersionNumber})", 
                    contractId, newCurrentVersionId, newCurrentVersion.VersionNumber);
                
                return true;
            }
            
            _logger?.LogWarning("Failed to update current version for contract {ContractId} - version {VersionId} not found", 
                contractId, newCurrentVersionId);
            
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating current version for contract {ContractId} to version {VersionId}", 
                contractId, newCurrentVersionId);
            throw;
        }
    }
}