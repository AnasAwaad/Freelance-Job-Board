using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class ClientRepository : GenericRepository<Client>, IClientRepository
{
	public ClientRepository(ApplicationDbContext context) : base(context)
	{
	}

	public async Task<Client> GetByUserIdAsync(string userId)
	{
		return await _context.Clients
			.Where(c => c.UserId == userId)
			.FirstOrDefaultAsync();
	}
}
