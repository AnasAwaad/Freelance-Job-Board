using FreelanceJobBoard.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
	public void Configure(EntityTypeBuilder<ApplicationUser> builder)
	{
		builder.HasKey(u => u.Id);

		builder.Property(u => u.FullName)
			.HasMaxLength(100);

		builder.Property(u => u.ProfileImageUrl)
			.HasMaxLength(500);
	}
}
