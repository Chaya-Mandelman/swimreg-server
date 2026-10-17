using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwimReg.Domain.Entities;
using SwimReg.Infrastructure.Persistence.Seed;

namespace SwimReg.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();

        // ה-enum נשמר כטקסט ("Admin" / "Instructor" / "Student")
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasData(SeedData.Users);
    }
}
