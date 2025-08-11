using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IContractChangeRequestRepository : IGenericRepository<ContractChangeRequest>
{
    Task<IEnumerable<ContractChangeRequest>> GetPendingRequestsAsync(int contractId);
    Task<IEnumerable<ContractChangeRequest>> GetRequestHistoryAsync(int contractId);
    Task<IEnumerable<ContractChangeRequest>> GetUserPendingRequestsAsync(string userId);
    Task<ContractChangeRequest?> GetRequestWithDetailsAsync(int requestId);
    Task<bool> HasPendingChangesAsync(int contractId);
}