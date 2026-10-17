using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwimReg.Domain.Entities;
using SwimReg.Infrastructure.Persistence.Seed;

namespace SwimReg.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.StartTime).IsRequired();

        // Pool 1:N Course
        builder.HasOne(c => c.Pool)
            .WithMany(p => p.Courses)
            .HasForeignKey(c => c.PoolId)
            .OnDelete(DeleteBehavior.Restrict);

        // User (Instructor) 1:N Course
        builder.HasOne(c => c.Instructor)
            .WithMany(u => u.InstructedCourses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optimistic Concurrency: מאפיין uint עם IsRowVersion ממופה ב-Npgsql לעמודת המערכת xmin.
        // PostgreSQL משנה את xmin בכל עדכון של השורה, ולכן לא נוצרת עמודה רגילה בשם Version.
        // EF מוסיף ל-UPDATE/DELETE תנאי WHERE xmin = @original, ואם לא עודכנה שורה → DbUpdateConcurrencyException
        builder.Property(c => c.Version).IsRowVersion();

        builder.HasData(SeedData.Courses);
    }
}
