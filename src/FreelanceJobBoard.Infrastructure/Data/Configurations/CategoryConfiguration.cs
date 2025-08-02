using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
	public void Configure(EntityTypeBuilder<Category> builder)
	{
		builder.HasKey(c => c.Id);

		builder.Property(c => c.Name)
			   .IsRequired()
			   .HasMaxLength(200);

		builder.Property(c => c.Description)
			   .HasMaxLength(1000);

		builder.HasMany(c => c.JobCategories)
			   .WithOne(jc => jc.Category)
			   .HasForeignKey(jc => jc.CategoryId)
			   .OnDelete(DeleteBehavior.Restrict);
	}
}

