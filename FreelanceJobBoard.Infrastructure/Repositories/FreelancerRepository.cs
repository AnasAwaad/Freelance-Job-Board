using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreelanceJobBoard.Infrastructure.Repositories
{
    public class FreelancerRepository : GenericRepository<Freelancer>, IFreelancerRepository
    {
        public FreelancerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Freelancer?> GetByUserIdAsync(string userId)
        {
            return await _context.Set<Freelancer>()
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.UserId == userId && f.IsActive);
        }

        public async Task<Freelancer?> GetFreelancerWithSkillsAsync(int freelancerId)
        {
            return await _context.Set<Freelancer>()
                .Include(f => f.User)
                .Include(f => f.FreelancerSkills)
                .Include(f => f.Certifications)
                .FirstOrDefaultAsync(f => f.Id == freelancerId && f.IsActive);
        }
    }
}