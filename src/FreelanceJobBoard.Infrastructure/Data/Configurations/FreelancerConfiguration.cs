using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class FreelancerConfiguration : IEntityTypeConfiguration<Freelancer>
{
	public void Configure(EntityTypeBuilder<Freelancer> builder)
	{
		builder.HasKey(f => f.Id);

		builder.Property(f => f.Bio)
			   .IsRequired();

		builder.Property(f => f.Description)
			   .HasMaxLength(500);

		builder.Property(f => f.HourlyRate)
			   .HasColumnType("decimal(18,2)");

		builder.Property(f => f.AverageRating)
			   .HasColumnType("decimal(3,2)");

		builder.HasOne(f => f.User)
			   .WithOne()
			   .HasForeignKey<Freelancer>(f => f.UserId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(f => f.Proposals)
			   .WithOne(p => p.Freelancer)
			   .HasForeignKey(p => p.FreelancerId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(f => f.FreelancerSkills)
			   .WithOne(fs => fs.Freelancer)
			   .HasForeignKey(fs => fs.FreelancerId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(f => f.Certifications)
			   .WithOne(c => c.Freelancer)
			   .HasForeignKey(c => c.FreelancerId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(f => f.Contracts)
			   .WithOne(c => c.Freelancer)
			   .HasForeignKey(c => c.FreelancerId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
