using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;

public class ContractChangeRequestConfiguration : IEntityTypeConfiguration<ContractChangeRequest>
{
    public void Configure(EntityTypeBuilder<ContractChangeRequest> builder)
    {
        builder.HasKey(ccr => ccr.Id);

        builder.Property(ccr => ccr.RequestedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(ccr => ccr.RequestedByRole)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ccr => ccr.ChangeDescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ccr => ccr.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ccr => ccr.ResponseByUserId)
            .HasMaxLength(450);

        builder.Property(ccr => ccr.ResponseByRole)
            .HasMaxLength(50);

        builder.Property(ccr => ccr.ResponseNotes)
            .HasMaxLength(1000);

        builder.Property(ccr => ccr.RequestDate)
            .IsRequired();

        builder.Property(ccr => ccr.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign key relationships
        builder.HasOne(ccr => ccr.Contract)
            .WithMany(c => c.ChangeRequests)
            .HasForeignKey(ccr => ccr.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ccr => ccr.FromVersion)
            .WithMany()
            .HasForeignKey(ccr => ccr.FromVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ccr => ccr.ProposedVersion)
            .WithMany()
            .HasForeignKey(ccr => ccr.ProposedVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ccr => ccr.ContractId);
        builder.HasIndex(ccr => ccr.RequestedByUserId);
        builder.HasIndex(ccr => ccr.Status);
        builder.HasIndex(ccr => ccr.RequestDate);
        builder.HasIndex(ccr => ccr.FromVersionId);
        builder.HasIndex(ccr => ccr.ProposedVersionId);
    }
}