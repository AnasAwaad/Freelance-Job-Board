using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;

namespace FreelanceJobBoard.Infrastructure.Repositories;
internal class ProposalRepository : GenericRepository<Proposal>, IProposalRepository
{
	public ProposalRepository(ApplicationDbContext context) : base(context)
	{
	}

}
