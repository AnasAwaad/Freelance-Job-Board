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

		builder.HasOne(n => n.User)
			   .WithMany(u => u.Notifications)
			   .HasForeignKey(n => n.UserId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(n => n.Template)
			   .WithMany(t => t.Notifications)
			   .HasForeignKey(n => n.NotificationTemplateId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}
