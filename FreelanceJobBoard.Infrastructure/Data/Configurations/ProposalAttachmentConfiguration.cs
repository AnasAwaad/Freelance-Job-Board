using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class ProposalAttachmentConfiguration : IEntityTypeConfiguration<ProposalAttachment>
{
	public void Configure(EntityTypeBuilder<ProposalAttachment> builder)
	{
		builder.HasKey(pa => new { pa.ProposalId, pa.AttachmentId });

		builder.HasOne(pa => pa.Proposal)
			   .WithMany(p => p.Attachments)
			   .HasForeignKey(pa => pa.ProposalId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(pa => pa.Attachment)
			   .WithMany(a => a.ProposalAttachments)
			   .HasForeignKey(pa => pa.AttachmentId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
