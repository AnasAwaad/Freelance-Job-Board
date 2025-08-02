using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class JobSkillConfiguration : IEntityTypeConfiguration<JobSkill>
{
	public void Configure(EntityTypeBuilder<JobSkill> builder)
	{
		builder.HasKey(js => new { js.JobId, js.SkillId });

		builder.HasOne(js => js.Job)
			   .WithMany(j => j.Skills)
			   .HasForeignKey(js => js.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(js => js.Skill)
			   .WithMany(s => s.JobSkills)
			   .HasForeignKey(js => js.SkillId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}

