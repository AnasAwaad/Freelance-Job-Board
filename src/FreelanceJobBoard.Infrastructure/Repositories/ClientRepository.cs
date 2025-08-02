using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;

internal class ClientRepository : GenericRepository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Client?> GetByUserIdAsync(string userId)
    {
        return await _context.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Client?> GetClientWithJobsAsync(int clientId)
    {
        return await _context.Clients
            .Include(c => c.Jobs)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == clientId);
    }
}