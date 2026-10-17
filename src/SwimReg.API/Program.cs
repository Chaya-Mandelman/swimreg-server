using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using SwimReg.API.Errors;
using SwimReg.API.Middleware;
using SwimReg.Application;
using SwimReg.Application.Settings;
using SwimReg.Infrastructure;
using SwimReg.Infrastructure.Persistence;

// לוגר של NLog שקיים עוד לפני שה-Host נבנה, כדי שגם שגיאת עלייה תירשם
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ---------- Logging ----------
    builder.Logging.ClearProviders();
    // RemoveLoggerFactoryFilter = false: רמות הלוג בסעיף Logging של appsettings חלות גם על NLog
    builder.Host.UseNLog(new NLogAspNetCoreOptions { RemoveLoggerFactoryFilter = false });

    // ---------- שכבות ----------
    // Lifetimes:
    //   DbContext, Repositories, Services → Scoped (מופע אחד לכל בקשה)
    //   TokenService, PasswordHasher      → Singleton (בלי state)
    // חשוב: Singleton אסור שיחזיק תלות Scoped (captive dependency).
    // ה-Singleton חי לאורך כל חיי האפליקציה, ולכן היה "לוכד" DbContext אחד ומשתמש בו בכל הבקשות במקביל:
    // DbContext לא thread-safe, הנתונים שבזיכרון שלו מתיישנים, והוא לא משוחרר אף פעם.
    // ב-Development ה-DI בודק את זה בעלייה (ValidateScopes) וזורק שגיאה.
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ---------- Controllers + ולידציה ----------
    builder.Services
        .AddControllers()
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
        .ConfigureApiBehaviorOptions(options =>
        {
            // איפה ModelState נבדק? [ApiController] בודק אותו אוטומטית לפני שה-Action רץ.
            // אם יש שגיאות (Data Annotations על ה-DTO, או JSON לא תקין), ה-Action לא נקרא בכלל
            // והפונקציה הזו בונה את תשובת ה-400 באותו JSON אחיד של ה-Middleware.
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value is { Errors.Count: > 0 })
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(err => string.IsNullOrEmpty(err.ErrorMessage) ? "ערך לא תקין" : err.ErrorMessage).ToArray());

                var body = ErrorResponse.Create(context.HttpContext, StatusCodes.Status400BadRequest, "הבקשה אינה תקינה", errors);
                return new BadRequestObjectResult(body) { ContentTypes = { "application/json" } };
            };
        });

    // ---------- JWT ----------
    var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
    if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
        throw new InvalidOperationException("Jwt:Key is missing or shorter than 32 bytes. Set it in User Secrets or as the Jwt__Key environment variable.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Email
            };

            // 401 (אין token / לא תקין / פג תוקף) ו-403 (מחובר בלי הרשאה) באותו JSON אחיד
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    await ErrorResponse.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "נדרשת התחברות עם token תקין");
                },
                OnForbidden = context =>
                    ErrorResponse.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden, "אין לך הרשאה לפעולה הזו")
            };
        });
    builder.Services.AddAuthorization();

    // ---------- CORS ----------
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options =>
        options.AddPolicy("Client", policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorrelationId.HeaderName, "Location")));

    // ---------- Reverse proxy (Render) ----------
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        // ב-Render ה-HTTPS מסתיים ב-Reverse Proxy שכתובתו לא ידועה מראש, לכן מקבלים headers מכל proxy
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // ---------- Swagger ----------
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "SwimReg API", Version = "v1" });

        // כפתור Authorize ב-Swagger: מדביקים את ה-token (בלי המילה Bearer)
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "הדבק כאן את ה-JWT שהתקבל מ-/api/auth/login"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // ---------- Pipeline (הסדר חשוב) ----------
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    if (app.Environment.IsProduction())
    {
        // מעדכן את Request.Scheme ל-https ואת ה-IP של הלקוח לפי ה-headers של ה-proxy
        app.UseForwardedHeaders();
    }

    if (app.Environment.IsDevelopment())
    {
        // HttpsRedirection רק בפיתוח: ב-Render ה-HTTPS מסתיים ב-Reverse Proxy,
        // והבקשה מגיעה לקונטיינר ב-HTTP. הפניה ל-HTTPS בתוך הקונטיינר עלולה ליצור לולאת הפניות,
        // ואין בה צורך כי Render כבר מגיש את האתר ב-HTTPS.
        app.UseHttpsRedirection();
    }

    app.UseCors("Client");

    // Swagger פעיל ב-Production לצורך הדגמה בפרויקט לימודי בלבד.
    // במערכת אמיתית מכבים אותו או מגנים עליו.
    if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ---------- Migrations ----------
    // רץ בעלייה כדי שגם ב-Render בסיס הנתונים יוקם ויתעדכן אוטומטית.
    // אם נכשל: נרשם Error והחריגה נזרקת שוב, כך שאפליקציה בלי DB מוכן לא עולה.
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<SwimRegDbContext>();
            await db.Database.MigrateAsync();
            startupLogger.LogInformation("Migrations applied");
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "Applying migrations failed");
            throw;
        }
    }

    await app.RunAsync();
}
// HostAbortedException נזרקת בכוונה ע"י כלי dotnet ef כשהוא בונה את ה-Host, זו לא שגיאה
catch (Exception ex) when (ex is not HostAbortedException)
{
    logger.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    // מרוקן את כל הלוגים שבתור לפני שהתהליך נסגר
    LogManager.Shutdown();
}
