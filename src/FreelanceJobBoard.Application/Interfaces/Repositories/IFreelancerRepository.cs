using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Interfaces.Repositories;

public interface IFreelancerRepository : IGenericRepository<Freelancer>
{
	Task<Freelancer?> GetByUserIdAsync(string userId);
	Task<Freelancer?> GetFreelancerWithSkillsAsync(int freelancerId);
	Task<Freelancer?> GetByUserIdWithDetailsAsync(string userId);
}
