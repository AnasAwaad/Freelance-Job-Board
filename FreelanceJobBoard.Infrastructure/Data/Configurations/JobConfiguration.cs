using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class JobConfiguration : IEntityTypeConfiguration<Job>
{
	public void Configure(EntityTypeBuilder<Job> builder)
	{
		builder.HasKey(j => j.Id);

		builder.Property(j => j.Title)
			   .HasMaxLength(255);

		builder.Property(j => j.Description)
			   .HasMaxLength(2000);

		builder.Property(j => j.Status)
			   .HasMaxLength(100);

		builder.Property(j => j.RequiredSkills)
			   .HasMaxLength(1000);

		builder.Property(j => j.Tags)
			   .HasMaxLength(500);

		builder.Property(j => j.BudgetMin)
			   .HasPrecision(18, 2);

		builder.Property(j => j.BudgetMax)
			   .HasPrecision(18, 2);

		builder.HasOne(j => j.Client)
			   .WithMany(c => c.Jobs)
			   .HasForeignKey(j => j.ClientId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(j => j.Categories)
			   .WithOne(jc => jc.Job)
			   .HasForeignKey(jc => jc.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(j => j.Attachments)
			   .WithOne(ja => ja.Job)
			   .HasForeignKey(ja => ja.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(j => j.Views)
			   .WithOne(jv => jv.Job)
			   .HasForeignKey(jv => jv.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(j => j.Skills)
			   .WithOne(js => js.Job)
			   .HasForeignKey(js => js.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(j => j.Proposals)
			   .WithOne(p => p.Job)
			   .HasForeignKey(p => p.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(j => j.Review)
			   .WithOne(r => r.Job)
			   .HasForeignKey<Review>(r => r.JobId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
