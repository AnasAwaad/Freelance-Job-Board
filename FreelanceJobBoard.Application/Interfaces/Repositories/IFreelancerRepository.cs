using FreelanceJobBoard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreelanceJobBoard.Application.Interfaces.Repositories
{
    public interface IFreelancerRepository : IGenericRepository<Freelancer>
    {
        Task<Freelancer?> GetByUserIdAsync(string userId);
        Task<Freelancer?> GetFreelancerWithSkillsAsync(int freelancerId);
    }
}
