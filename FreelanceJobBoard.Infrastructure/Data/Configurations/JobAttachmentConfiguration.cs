using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class JobAttachmentConfiguration : IEntityTypeConfiguration<JobAttachment>
{
	public void Configure(EntityTypeBuilder<JobAttachment> builder)
	{
		builder.HasKey(ja => new { ja.JobId, ja.AttachmentId });

		builder.HasOne(ja => ja.Job)
			   .WithMany(j => j.Attachments)
			   .HasForeignKey(ja => ja.JobId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(ja => ja.Attachment)
			   .WithMany(a => a.JobAttachments)
			   .HasForeignKey(ja => ja.AttachmentId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
