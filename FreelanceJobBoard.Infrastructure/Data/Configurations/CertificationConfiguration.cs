using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
	public void Configure(EntityTypeBuilder<Certification> builder)
	{
		builder.HasKey(c => c.Id);

		builder.Property(c => c.Name)
			   .IsRequired()
			   .HasMaxLength(255);

		builder.Property(c => c.Provider)
			   .HasMaxLength(255);

		builder.Property(c => c.Description)
			   .HasMaxLength(1000);

		builder.Property(c => c.CertificationLink)
			   .HasMaxLength(500);

		builder.Property(c => c.DateEarned)
			   .IsRequired();

		builder.HasOne(c => c.Freelancer)
			   .WithMany(f => f.Certifications)
			   .HasForeignKey(c => c.FreelancerId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
