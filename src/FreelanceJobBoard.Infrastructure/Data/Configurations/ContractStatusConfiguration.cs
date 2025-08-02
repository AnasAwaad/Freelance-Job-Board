using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreelanceJobBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelanceJobBoard.Infrastructure.Data.Configurations;
public class ContractStatusConfiguration : IEntityTypeConfiguration<ContractStatus>
{
    public void Configure(EntityTypeBuilder<ContractStatus> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);

        builder.HasData(
            new ContractStatus { Id = 1, Name = "Pending" },
            new ContractStatus { Id = 2, Name = "Active" },
            new ContractStatus { Id = 3, Name = "Completed" },
            new ContractStatus { Id = 4, Name = "Cancelled" }
        );
    }
}