using FreelanceJobBoard.Application.Features.User.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;
public interface IClientRepository : IGenericRepository<Client>
{
	Task<Client> GetByUserIdAsync(string userId);
	Task<Client?> GetByUserIdWithDetailsAsync(string userId);
	Task<IEnumerable<TopClientDto>> GetTopClientsAsync(int numOfClients);
	Task<int> GetTotalNumbers();
}
