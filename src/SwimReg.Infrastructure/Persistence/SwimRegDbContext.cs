using Microsoft.EntityFrameworkCore;
using SwimReg.Domain.Entities;

namespace SwimReg.Infrastructure.Persistence;

public class SwimRegDbContext : DbContext
{
    public SwimRegDbContext(DbContextOptions<SwimRegDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Pool> Pools => Set<Pool>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // טוען את כל מחלקות ה-IEntityTypeConfiguration מתיקיית Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SwimRegDbContext).Assembly);
    }
}
