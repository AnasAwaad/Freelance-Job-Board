using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class FreelancerSkillConfiguration : IEntityTypeConfiguration<FreelancerSkill>
{
	public void Configure(EntityTypeBuilder<FreelancerSkill> builder)
	{
		builder.HasKey(fs => new { fs.FreelancerId, fs.SkillId });

		builder.HasOne(fs => fs.Freelancer)
			   .WithMany(f => f.FreelancerSkills)
			   .HasForeignKey(fs => fs.FreelancerId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(fs => fs.Skill)
			   .WithMany(s => s.FreelancerSkills)
			   .HasForeignKey(fs => fs.SkillId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}

