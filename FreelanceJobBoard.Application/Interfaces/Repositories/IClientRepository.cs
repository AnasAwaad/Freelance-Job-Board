 using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IClientRepository : IGenericRepository<Client>
{
    Task<Client?> GetByUserIdAsync(string userId);
    Task<Client?> GetClientWithJobsAsync(int clientId);
}