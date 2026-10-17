using Microsoft.EntityFrameworkCore;
using SwimReg.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SwimReg.Tests.Integration;

// מרים PostgreSQL אמיתי בקונטיינר (דורש Docker), כי xmin קיים רק ב-PostgreSQL.
// בסיס הנתונים נקי, וה-Migrations רצות עליו פעם אחת לכל מחלקת בדיקות.
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public SwimRegDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<SwimRegDbContext>().UseNpgsql(_container.GetConnectionString()).Options);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
