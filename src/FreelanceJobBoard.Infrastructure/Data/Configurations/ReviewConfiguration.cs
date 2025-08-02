using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
	public void Configure(EntityTypeBuilder<Review> builder)
	{
		builder.HasKey(r => r.Id);

		builder.Property(r => r.Rating)
			   .IsRequired();

		builder.Property(r => r.Comment)
			   .HasMaxLength(1000);

		builder.Property(r => r.ReviewType)
			   .HasMaxLength(100)
			   .IsRequired();

		builder.Property(r => r.IsVisible)
			   .IsRequired();

		builder.HasOne(r => r.Job)
			   .WithOne(j => j.Review)
			   .HasForeignKey<Review>(r => r.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne<ApplicationUser>()
			   .WithMany()
			   .HasForeignKey(r => r.ReviewerId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne<ApplicationUser>()
			   .WithMany()
			   .HasForeignKey(r => r.RevieweeId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
