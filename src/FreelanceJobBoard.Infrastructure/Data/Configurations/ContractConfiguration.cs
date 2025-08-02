using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
	public void Configure(EntityTypeBuilder<Contract> builder)
	{
		builder.HasKey(c => c.Id);

		builder.Property(c => c.StartTime)
			   .IsRequired();

		builder.Property(c => c.AgreedPaymentType)
			   .HasMaxLength(100);

		builder.Property(c => c.PaymentAmount)
			   .HasColumnType("decimal(10,2)")
			   .IsRequired();

		builder.Property(c => c.EndTime);

		builder.Property(c => c.ContractStatusId).IsRequired();

		builder.HasOne(c => c.ContractStatus)
			.WithMany(s => s.Contracts)
			.HasForeignKey(c => c.ContractStatusId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(c => c.Proposal)
			   .WithMany()
			   .HasForeignKey(c => c.ProposalId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(c => c.Client)
			   .WithMany(c => c.Contracts)
			   .HasForeignKey(c => c.ClientId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(c => c.Freelancer)
			   .WithMany(f => f.Contracts)
			   .HasForeignKey(c => c.FreelancerId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}

