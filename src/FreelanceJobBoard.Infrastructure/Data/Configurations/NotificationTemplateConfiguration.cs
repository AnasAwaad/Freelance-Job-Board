using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
	public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
	{
		builder.HasKey(nt => nt.Id);

		builder.Property(nt => nt.TemplateName)
			   .IsRequired()
			   .HasMaxLength(100);

		builder.Property(nt => nt.TemplateTitle)
			   .IsRequired()
			   .HasMaxLength(200);

		builder.Property(nt => nt.TemplateMessage)
			   .IsRequired()
			   .HasMaxLength(1000);

		builder.HasMany(nt => nt.Notifications)
			   .WithOne(n => n.Template)
			   .HasForeignKey(n => n.NotificationTemplateId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}

