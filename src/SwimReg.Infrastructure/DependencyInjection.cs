using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Interfaces.Security;
using SwimReg.Application.Settings;
using SwimReg.Infrastructure.Persistence;
using SwimReg.Infrastructure.Repositories;
using SwimReg.Infrastructure.Security;

namespace SwimReg.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ה-connection string נקרא רק מהקונפיגורציה (User Secrets / משתני סביבה), אף פעם לא מהקוד
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        connectionString = NormalizeConnectionString(connectionString);

        // AddDbContext רושם את ה-DbContext כ-Scoped: מופע אחד לכל בקשת HTTP
        services.AddDbContext<SwimRegDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPoolRepository, PoolRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Singleton: בלי state ובלי תלות בשירותים Scoped
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }

    // Render נותן כתובת בפורמט postgres://USER:PASSWORD@HOST:PORT/DATABASE, ו-Npgsql לא מקבל URI.
    // אם הגיעה כתובת כזו, ממירים אותה לפורמט key=value. כל פורמט אחר מוחזר כמו שהוא.
    private static string NormalizeConnectionString(string value)
    {
        if (!value.StartsWith("postgres://") && !value.StartsWith("postgresql://"))
            return value;

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            // Internal URL של Render עובד גם בלי SSL; External דורש SSL. Prefer מתאים לשניהם.
            SslMode = Npgsql.SslMode.Prefer
        };
        return builder.ConnectionString;
    }
}
