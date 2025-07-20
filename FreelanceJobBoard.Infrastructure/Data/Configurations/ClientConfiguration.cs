using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
	public void Configure(EntityTypeBuilder<Client> builder)
	{
		builder.HasKey(c => c.Id);

		builder.OwnsOne(c => c.Company);

		builder.Property(c => c.AverageRating)
			   .HasPrecision(5, 2)
			   .IsRequired();

		builder.Property(c => c.TotalReviews)
			   .IsRequired();

		builder.HasOne(c => c.User)
			   .WithOne()
			   .HasForeignKey<Client>(c => c.UserId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(c => c.Jobs)
			   .WithOne(j => j.Client)
			   .HasForeignKey(j => j.ClientId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(c => c.Proposals)
			   .WithOne(p => p.Client)
			   .HasForeignKey(p => p.ClientId)
			   .OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(c => c.Contracts)
			   .WithOne(ct => ct.Client)
			   .HasForeignKey(ct => ct.ClientId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}

