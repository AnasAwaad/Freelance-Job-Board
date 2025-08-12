using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;

public class ContractVersionRepository : GenericRepository<ContractVersion>, IContractVersionRepository
{
    public ContractVersionRepository(ApplicationDbContext context, ILogger<GenericRepository<ContractVersion>>? logger = null) : base(context, logger)
    {
    }

    public async Task<ContractVersion?> GetCurrentVersionAsync(int contractId)
    {
        try
        {
            _logger?.LogDebug("?? Getting current version | ContractId={ContractId}", contractId);
            
            var currentVersion = await _context.ContractVersions
                .Where(v => v.ContractId == contractId && v.IsCurrentVersion && v.IsActive)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();

            if (currentVersion != null)
            {
                _logger?.LogDebug("? Current version found | ContractId={ContractId}, VersionId={VersionId}, VersionNumber={VersionNumber}", 
                    contractId, currentVersion.Id, currentVersion.VersionNumber);
            }
            else
            {
                _logger?.LogDebug("? No current version found | ContractId={ContractId}", contractId);
            }

            return currentVersion;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error getting current version | ContractId={ContractId}", contractId);
            throw;
        }
    }

    public async Task<IEnumerable<ContractVersion>> GetVersionHistoryAsync(int contractId)
    {
        try
        {
            _logger?.LogDebug("?? Getting version history | ContractId={ContractId}", contractId);
            
            var versions = await _context.ContractVersions
                .Where(v => v.ContractId == contractId && v.IsActive)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            _logger?.LogDebug("? Version history retrieved | ContractId={ContractId}, VersionCount={VersionCount}", 
                contractId, versions.Count());

            return versions;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error getting version history | ContractId={ContractId}", contractId);
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
            _logger?.LogError(ex, "? Error getting version by number | ContractId={ContractId}, VersionNumber={VersionNumber}", contractId, versionNumber);
            throw;
        }
    }

    public async Task CreateNewVersionAsync(ContractVersion version)
    {
        try
        {
            // Mark all other versions as not current
            var existingVersions = await _context.ContractVersions
                .Where(v => v.ContractId == version.ContractId && v.IsCurrentVersion)
                .ToListAsync();

            foreach (var existingVersion in existingVersions)
            {
                existingVersion.IsCurrentVersion = false;
            }

            // Add the new version
            await _context.ContractVersions.AddAsync(version);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error creating new version | ContractId={ContractId}", version.ContractId);
            throw;
        }
    }

    public async Task<int> GetNextVersionNumberAsync(int contractId)
    {
        try
        {
            var lastVersion = await _context.ContractVersions
                .Where(v => v.ContractId == contractId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync();

            return (lastVersion?.VersionNumber ?? 0) + 1;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error getting next version number | ContractId={ContractId}", contractId);
            throw;
        }
    }

    public async Task<bool> UpdateCurrentVersionAsync(int contractId, int newCurrentVersionId)
    {
        try
        {
            _logger?.LogDebug("?? Updating current version | ContractId={ContractId}, NewVersionId={NewVersionId}", contractId, newCurrentVersionId);

            // Mark all versions as not current
            var allVersions = await _context.ContractVersions
                .Where(v => v.ContractId == contractId)
                .ToListAsync();

            foreach (var version in allVersions)
            {
                version.IsCurrentVersion = false;
            }

            // Mark the new version as current
            var newCurrentVersion = allVersions.FirstOrDefault(v => v.Id == newCurrentVersionId);
            if (newCurrentVersion != null)
            {
                newCurrentVersion.IsCurrentVersion = true;
                _logger?.LogDebug("? Current version updated | ContractId={ContractId}, NewVersionId={NewVersionId}", contractId, newCurrentVersionId);
                return true;
            }
            else
            {
                _logger?.LogWarning("?? New current version not found | ContractId={ContractId}, NewVersionId={NewVersionId}", contractId, newCurrentVersionId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Error updating current version | ContractId={ContractId}, NewVersionId={NewVersionId}", contractId, newCurrentVersionId);
            throw;
        }
    }
}