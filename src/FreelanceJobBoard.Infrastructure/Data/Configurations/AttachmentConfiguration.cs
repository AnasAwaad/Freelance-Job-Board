using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
	public void Configure(EntityTypeBuilder<Attachment> builder)
	{
		builder.HasKey(a => a.Id);

		builder.Property(a => a.FileName)
			   .IsRequired()
			   .HasMaxLength(255);

		builder.Property(a => a.FilePath)
			   .IsRequired()
			   .HasMaxLength(1000);

		builder.Property(a => a.FileType)
			   .IsRequired()
			   .HasMaxLength(100);

		builder.Property(a => a.FileSize)
			   .IsRequired();

		builder.HasMany(a => a.ProposalAttachments)
			   .WithOne(pa => pa.Attachment)
			   .HasForeignKey(pa => pa.AttachmentId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(a => a.JobAttachments)
			   .WithOne(ja => ja.Attachment)
			   .HasForeignKey(ja => ja.AttachmentId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
