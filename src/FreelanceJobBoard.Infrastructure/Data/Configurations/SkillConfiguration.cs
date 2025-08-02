using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
	public void Configure(EntityTypeBuilder<Skill> builder)
	{
		builder.HasKey(s => s.Id);

		builder.Property(s => s.Name)
			   .IsRequired()
			   .HasMaxLength(255);

		builder.HasMany(s => s.JobSkills)
			   .WithOne(js => js.Skill)
			   .HasForeignKey(js => js.SkillId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(s => s.FreelancerSkills)
			   .WithOne(fs => fs.Skill)
			   .HasForeignKey(fs => fs.SkillId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}