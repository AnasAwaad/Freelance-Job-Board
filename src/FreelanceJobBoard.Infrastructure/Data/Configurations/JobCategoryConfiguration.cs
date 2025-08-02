using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class JobCategoryConfiguration : IEntityTypeConfiguration<JobCategory>
{
	public void Configure(EntityTypeBuilder<JobCategory> builder)
	{
		builder.HasKey(jc => new { jc.JobId, jc.CategoryId });

		builder.HasOne(jc => jc.Job)
			   .WithMany(j => j.Categories)
			   .HasForeignKey(jc => jc.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(jc => jc.Category)
			   .WithMany(c => c.JobCategories)
			   .HasForeignKey(jc => jc.CategoryId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}

