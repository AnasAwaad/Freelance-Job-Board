using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IContractVersionRepository : IGenericRepository<ContractVersion>
{
    Task<ContractVersion?> GetCurrentVersionAsync(int contractId);
    Task<IEnumerable<ContractVersion>> GetVersionHistoryAsync(int contractId);
    Task<ContractVersion?> GetVersionByNumberAsync(int contractId, int versionNumber);
    Task<int> GetNextVersionNumberAsync(int contractId);
    Task<bool> UpdateCurrentVersionAsync(int contractId, int newCurrentVersionId);
}