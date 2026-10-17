using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwimReg.Domain.Entities;
using SwimReg.Infrastructure.Persistence.Seed;

namespace SwimReg.Infrastructure.Persistence.Configurations;

public class PoolConfiguration : IEntityTypeConfiguration<Pool>
{
    public void Configure(EntityTypeBuilder<Pool> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Address).IsRequired().HasMaxLength(200);

        builder.HasData(SeedData.Pools);
    }
}
