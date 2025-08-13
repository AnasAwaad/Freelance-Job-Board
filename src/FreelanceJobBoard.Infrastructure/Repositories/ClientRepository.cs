using FreelanceJobBoard.Application.Features.User.DTOs;
using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class ClientRepository : GenericRepository<Client>, IClientRepository
{
	public ClientRepository(ApplicationDbContext context, ILogger<GenericRepository<Client>>? logger = null) : base(context, logger)
	{
	}

	public async Task<Client?> GetByUserIdAsync(string userId)
	{
		return await _context.Clients
			.Include(c => c.User)
			.Include(c => c.Company)
			.FirstOrDefaultAsync(c => c.UserId == userId);
	}

	public async Task<Client?> GetByUserIdWithDetailsAsync(string userId)
	{
		return await _context.Clients
			.Include(c => c.User)
			.Include(c => c.Company)
			.Include(c => c.Jobs)
			.FirstOrDefaultAsync(c => c.UserId == userId);
	}

	public async Task<IEnumerable<TopClientDto>> GetTopClientsAsync(int numOfClients)
	{
		return await _context.Clients.Select(c => new TopClientDto
		{
			Id = c.Id,
			FullName = c.User.FullName,
			ProposalsCount = c.Jobs.SelectMany(j => j.Proposals).Count(),
			Company = c.Company.Name,
			ProfileImageUrl = c.User.ProfileImageUrl
		}).OrderByDescending(c => c.ProposalsCount)
		.Where(j => j.ProposalsCount > 0)
		.Take(numOfClients)
		.ToListAsync();

	}

	public async Task<int> GetTotalNumbers()
	{
		return await _context.Clients.CountAsync();
	}
}