using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Skill : BaseEntity
{
	public int Id { get; set; }
	public string Name { get; set; }

	public ICollection<JobSkill> JobSkills { get; set; }
	public ICollection<FreelancerSkill> FreelancerSkills { get; set; }
}
