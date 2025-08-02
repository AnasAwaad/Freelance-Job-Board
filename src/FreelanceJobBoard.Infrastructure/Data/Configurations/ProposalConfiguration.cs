using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
	public void Configure(EntityTypeBuilder<Proposal> builder)
	{
		builder.HasKey(p => p.Id);

		builder.Property(p => p.CoverLetter)
			   .HasMaxLength(4000);

		builder.Property(p => p.BidAmount)
			   .HasColumnType("decimal(18,2)")
			   .IsRequired();

		builder.Property(p => p.EstimatedTimelineDays)
			   .IsRequired();

		builder.Property(p => p.Status)
			   .HasMaxLength(100);

		builder.HasOne(p => p.Job)
			   .WithMany(j => j.Proposals)
			   .HasForeignKey(p => p.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(p => p.Client)
			   .WithMany(c => c.Proposals)
			   .HasForeignKey(p => p.ClientId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(p => p.Freelancer)
			   .WithMany(f => f.Proposals)
			   .HasForeignKey(p => p.FreelancerId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(p => p.Contract)
			   .WithOne(c => c.Proposal)
			   .HasForeignKey<Contract>(c => c.ProposalId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(p => p.Attachments)
			   .WithOne(pa => pa.Proposal)
			   .HasForeignKey(pa => pa.ProposalId)
			   .OnDelete(DeleteBehavior.Cascade);
	}
}
