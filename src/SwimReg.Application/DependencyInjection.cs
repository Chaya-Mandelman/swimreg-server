using Microsoft.Extensions.DependencyInjection;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Application.Mapping;
using SwimReg.Application.Services;

namespace SwimReg.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // Scoped: תלויים ב-Repositories, שתלויים ב-DbContext (Scoped)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPoolService, PoolService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IRegistrationService, RegistrationService>();

        return services;
    }
}
