using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class FreelancerSkill : BaseEntity
{
	public int FreelancerId { get; set; }
	public int SkillId { get; set; }

	public Freelancer Freelancer { get; set; }
	public Skill Skill { get; set; }
}
