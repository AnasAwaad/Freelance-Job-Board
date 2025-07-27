using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Infrastructure.Repositories;

public class ClientRepository : GenericRepository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context)
    {
        
    }
    public async Task<Client?> GetByUserIdAsync(string userId)
    {
        return await _context.Set<Client>()
            .Include(c => c.User)
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
    }

    public async Task<Client?> GetClientWithCompanyAsync(int clientId)
    {
        return await _context.Set<Client>()
            .Include(c => c.User)
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == clientId && c.IsActive);
    }
}