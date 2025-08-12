using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
	public void Configure(EntityTypeBuilder<Notification> builder)
	{
		builder.HasKey(n => n.Id);

		builder.Property(n => n.Title)
			   .HasMaxLength(200)
			   .IsRequired();

		builder.Property(n => n.Message)
			   .HasMaxLength(1000)
			   .IsRequired();

		builder.Property(n => n.IsRead)
			   .IsRequired();

		builder.Property(n => n.ReadAt)
			   .IsRequired(false);

		// Enhanced tracking fields
		builder.Property(n => n.RecipientUserId)
			   .HasMaxLength(450)
			   .IsRequired();

		builder.Property(n => n.SenderUserId)
			   .HasMaxLength(450)
			   .IsRequired(false);

		builder.Property(n => n.Type)
			   .HasMaxLength(100)
			   .IsRequired();

		// UI properties
		builder.Property(n => n.Icon)
			   .HasMaxLength(100)
			   .IsRequired(false);

		builder.Property(n => n.Color)
			   .HasMaxLength(50)
			   .IsRequired(false);

		builder.Property(n => n.IsUrgent)
			   .IsRequired()
			   .HasDefaultValue(false);

		// Metadata properties
		builder.Property(n => n.ExpiryDate)
			   .IsRequired(false);

		builder.Property(n => n.IsEmailSent)
			   .IsRequired()
			   .HasDefaultValue(false);

		builder.Property(n => n.ActionUrl)
			   .HasMaxLength(1000)
			   .IsRequired(false);

		builder.Property(n => n.Data)
			   .HasColumnType("nvarchar(max)")
			   .IsRequired(false);

		// Entity reference properties
		builder.Property(n => n.JobId)
			   .IsRequired(false);

		builder.Property(n => n.ProposalId)
			   .IsRequired(false);

		builder.Property(n => n.ContractId)
			   .IsRequired(false);

		builder.Property(n => n.ReviewId)
			   .IsRequired(false);

		// Backward compatibility
		builder.Property(n => n.UserId)
			   .HasMaxLength(450)
			   .IsRequired();

		// Configure relationships
		// Primary user relationship (backward compatibility)
		builder.HasOne(n => n.User)
			   .WithMany(u => u.Notifications)
			   .HasForeignKey(n => n.UserId)
			   .OnDelete(DeleteBehavior.Restrict);

		// Recipient user relationship
		builder.HasOne(n => n.RecipientUser)
			   .WithMany() // No back navigation to avoid confusion
			   .HasForeignKey(n => n.RecipientUserId)
			   .OnDelete(DeleteBehavior.Restrict);

		// Sender user relationship (optional)
		builder.HasOne(n => n.SenderUser)
			   .WithMany() // No back navigation to avoid confusion
			   .HasForeignKey(n => n.SenderUserId)
			   .OnDelete(DeleteBehavior.Restrict)
			   .IsRequired(false);

		// Template relationship
		builder.HasOne(n => n.Template)
			   .WithMany(t => t.Notifications)
			   .HasForeignKey(n => n.NotificationTemplateId)
			   .OnDelete(DeleteBehavior.Restrict);

		// Entity relationships (optional)
		builder.HasOne(n => n.Job)
			   .WithMany() // No back navigation to avoid circular references
			   .HasForeignKey(n => n.JobId)
			   .OnDelete(DeleteBehavior.Restrict)
			   .IsRequired(false);

		builder.HasOne(n => n.Proposal)
			   .WithMany() // No back navigation to avoid circular references
			   .HasForeignKey(n => n.ProposalId)
			   .OnDelete(DeleteBehavior.Restrict)
			   .IsRequired(false);

		builder.HasOne(n => n.Contract)
			   .WithMany() // No back navigation to avoid circular references
			   .HasForeignKey(n => n.ContractId)
			   .OnDelete(DeleteBehavior.Restrict)
			   .IsRequired(false);

		builder.HasOne(n => n.Review)
			   .WithMany() // No back navigation to avoid circular references
			   .HasForeignKey(n => n.ReviewId)
			   .OnDelete(DeleteBehavior.Restrict)
			   .IsRequired(false);

		// Indexes for performance
		builder.HasIndex(n => n.RecipientUserId);
		builder.HasIndex(n => n.SenderUserId);
		builder.HasIndex(n => n.Type);
		builder.HasIndex(n => n.IsRead);
		builder.HasIndex(n => n.IsUrgent);
		builder.HasIndex(n => n.CreatedOn);
		builder.HasIndex(n => new { n.RecipientUserId, n.IsRead });
		builder.HasIndex(n => new { n.RecipientUserId, n.Type });
		builder.HasIndex(n => new { n.JobId, n.Type });
		builder.HasIndex(n => new { n.ContractId, n.Type });
	}
}
