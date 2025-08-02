using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class JobSkill : BaseEntity
{
	public int JobId { get; set; }
	public int SkillId { get; set; }

	public Job Job { get; set; }
	public Skill Skill { get; set; }
}
