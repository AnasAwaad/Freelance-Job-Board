using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IContractRepository : IGenericRepository<Contract>
{
    Task<Contract?> GetContractByProposalIdAsync(int proposalId);
    Task<IEnumerable<Contract>> GetContractsByClientIdAsync(int clientId);
    Task<IEnumerable<Contract>> GetContractsByFreelancerIdAsync(int freelancerId);
    Task<Contract?> GetContractWithDetailsAsync(int contractId);
    Task<IEnumerable<Contract>> GetActiveContractsAsync();
}