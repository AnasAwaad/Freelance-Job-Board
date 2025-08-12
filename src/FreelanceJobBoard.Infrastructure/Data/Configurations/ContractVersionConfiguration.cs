using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;

public class ContractVersionConfiguration : IEntityTypeConfiguration<ContractVersion>
{
    public void Configure(EntityTypeBuilder<ContractVersion> builder)
    {
        builder.HasKey(cv => cv.Id);

        builder.Property(cv => cv.VersionNumber)
            .IsRequired();

        builder.Property(cv => cv.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cv => cv.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cv => cv.PaymentAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(cv => cv.PaymentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cv => cv.Deliverables)
            .HasMaxLength(2000);

        builder.Property(cv => cv.TermsAndConditions)
            .HasMaxLength(5000);

        builder.Property(cv => cv.AdditionalNotes)
            .HasMaxLength(1000);

        builder.Property(cv => cv.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(cv => cv.CreatedByRole)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(cv => cv.ChangeReason)
            .HasMaxLength(500);

        builder.Property(cv => cv.CreatedOn)
            .IsRequired();

        builder.Property(cv => cv.IsCurrentVersion)
            .IsRequired();

        builder.Property(cv => cv.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign key relationships
        builder.HasOne(cv => cv.Contract)
            .WithMany(c => c.Versions)
            .HasForeignKey(cv => cv.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cv => cv.ContractId);
        builder.HasIndex(cv => new { cv.ContractId, cv.VersionNumber })
            .IsUnique();
        builder.HasIndex(cv => new { cv.ContractId, cv.IsCurrentVersion });
    }
}