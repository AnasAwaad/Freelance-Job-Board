using FreelanceJobBoard.Application.Interfaces.Repositories;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Infrastructure.Repositories;
public class SkillRepository : GenericRepository<Skill>, ISkillRepository

{
	public SkillRepository(ApplicationDbContext context, ILogger<GenericRepository<Skill>>? logger = null) : base(context, logger)
	{
	}

	public async Task<List<Skill>> GetSkillsByIdsAsync(IEnumerable<int> skillIds)
	{
		_logger?.LogDebug("🔍 Getting skills by IDs | SkillIds={SkillIds}", string.Join(",", skillIds));
		
		try
		{
			var skills = await _context.Skills
				.Where(s => skillIds.Contains(s.Id))
				.ToListAsync();

			_logger?.LogDebug("✅ Skills retrieved | RequestedCount={RequestedCount}, FoundCount={FoundCount}", 
				skillIds.Count(), skills.Count);
			
			return skills;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to get skills by IDs | SkillIds={SkillIds}", string.Join(",", skillIds));
			throw;
		}
	}

    public async Task<List<Skill>> GetSkillsByNamesAsync(List<string> skillNames)
    {
		_logger?.LogDebug("🔍 Getting skills by names | SkillNames={SkillNames}", string.Join(",", skillNames));
		
		try
		{
			var skills = await _context.Skills
				.Where(s => skillNames.Contains(s.Name))
				.ToListAsync();

			_logger?.LogDebug("✅ Skills retrieved by names | RequestedCount={RequestedCount}, FoundCount={FoundCount}", 
				skillNames.Count, skills.Count);
			
			return skills;
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "❌ Failed to get skills by names | SkillNames={SkillNames}", string.Join(",", skillNames));
			throw;
		}
    }
}
