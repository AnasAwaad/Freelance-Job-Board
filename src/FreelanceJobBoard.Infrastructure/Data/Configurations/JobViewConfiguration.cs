using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class JobViewConfiguration : IEntityTypeConfiguration<JobView>
{
	public void Configure(EntityTypeBuilder<JobView> builder)
	{
		builder.HasKey(jv => jv.Id);

		builder.Property(jv => jv.ViewedAt)
			   .IsRequired();

		builder.Property(jv => jv.IpAddress)
			   .HasMaxLength(100);

		builder.HasOne(jv => jv.Job)
			   .WithMany(j => j.Views)
			   .HasForeignKey(jv => jv.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(jv => jv.User)
			   .WithMany()
			   .HasForeignKey(jv => jv.UserId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
