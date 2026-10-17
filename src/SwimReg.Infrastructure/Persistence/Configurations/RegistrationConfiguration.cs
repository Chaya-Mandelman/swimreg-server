using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwimReg.Domain.Entities;
using SwimReg.Infrastructure.Persistence.Seed;

namespace SwimReg.Infrastructure.Persistence.Configurations;

public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RegisteredAt).IsRequired();

        // מתאמן לא יכול להירשם פעמיים לאותו קורס, גם לא במקביל
        builder.HasIndex(r => new { r.UserId, r.CourseId }).IsUnique();

        // User N:M Course דרך Registration
        builder.HasOne(r => r.User)
            .WithMany(u => u.Registrations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Course)
            .WithMany(c => c.Registrations)
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(SeedData.Registrations);
    }
}
