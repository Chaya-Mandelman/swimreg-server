using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Courses;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Mapping;
using SwimReg.Application.Services;
using SwimReg.Domain.Entities;
using SwimReg.Domain.Enums;
using SwimReg.Domain.Exceptions;
using SwimReg.Infrastructure.Persistence;
using SwimReg.Infrastructure.Repositories;

namespace SwimReg.Tests.Integration;

public class ConcurrencyTests : IClassFixture<PostgresFixture>
{
    // משתמשים מה-Seed
    private const int InstructorId = 2;
    private const int StudentA = 4;
    private const int StudentB = 5;

    private readonly PostgresFixture _db;

    public ConcurrencyTests(PostgresFixture db) => _db = db;

    // כל בדיקה יוצרת קורס משלה, כדי שהבדיקות לא ישפיעו זו על זו
    private async Task<int> CreateCourseAsync(int max)
    {
        await using var context = _db.CreateContext();
        var course = new Course
        {
            Name = $"Test course {Guid.NewGuid():N}", Level = CourseLevel.Beginner, DayOfWeek = DayOfWeek.Friday,
            StartTime = new TimeOnly(10, 0), MaxParticipants = max, CurrentCount = 0, PoolId = 1, InstructorId = InstructorId
        };
        context.Courses.Add(course);
        await context.SaveChangesAsync();
        return course.Id;
    }

    // שכבה: Infrastructure / EF Core בלבד. הבדיקה עובדת ישירות מול שני מופעים של SwimRegDbContext
    // וקוראת ישירות ל-DbContext.SaveChangesAsync(), בלי Repository.
    // לכן היא מצפה ל-DbUpdateConcurrencyException המקורית של EF: היא מוכיחה שהמיפוי של Version ל-xmin
    // באמת עובד מול PostgreSQL, ושאין מי שמתרגם את החריגה בדרך.
    [Fact]
    public async Task Concurrency_TwoContextsUpdateSameCourse_SecondThrows()
    {
        // 1. Migrations רצו ב-Fixture על DB נקי. יוצרים קורס
        var courseId = await CreateCourseAsync(max: 5);

        // 2. שני DbContext נפרדים טוענים את אותו קורס (עם אותו xmin)
        await using var first = _db.CreateContext();
        await using var second = _db.CreateContext();
        var courseInFirst = await first.Courses.SingleAsync(c => c.Id == courseId);
        var courseInSecond = await second.Courses.SingleAsync(c => c.Id == courseId);
        Assert.Equal(courseInFirst.Version, courseInSecond.Version);

        // 3. שניהם מעדכנים את CurrentCount
        courseInFirst.CurrentCount = 1;
        courseInSecond.CurrentCount = 2;

        // 4. הראשון שומר בהצלחה (ה-xmin בשורה משתנה)
        await first.SaveChangesAsync();

        // 5. השני שולח UPDATE ... WHERE xmin = הישן, אף שורה לא מתעדכנת → חריגה
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());

        // 6. ב-DbContext שלישי: נשמר רק העדכון הראשון
        await using var verify = _db.CreateContext();
        var saved = await verify.Courses.AsNoTracking().SingleAsync(c => c.Id == courseId);
        Assert.Equal(1, saved.CurrentCount);
    }

    // שכבה: Application + Infrastructure יחד. הבדיקה עוברת דרך RegistrationService האמיתי
    // ו-RegistrationRepository / CourseRepository האמיתיים, מול PostgreSQL אמיתי.
    // ה-Repository תופס את DbUpdateConcurrencyException וזורק ConcurrencyConflictException (חריגת Domain),
    // ולכן הבדיקה מצפה דווקא ל-ConcurrencyConflictException, החריגה שה-Service וה-Middleware רואים.
    // בנוסף היא מוכיחה אטומיות: אחרי ההתנגשות לא נשמרה הרשמה, והמכסה לא השתנתה בגללנו.
    [Fact]
    public async Task Register_WhenConflict_NoRegistrationSaved()
    {
        var courseId = await CreateCourseAsync(max: 2);

        await using var context = _db.CreateContext();
        var realCourses = new CourseRepository(context);
        // מתאמן אחר נרשם "באמצע": אחרי שה-Service טען את הקורס ולפני שהוא שומר
        var courses = new CompetingRegistrationCourseRepository(realCourses, _db, StudentB);
        var registrations = new RegistrationRepository(context);
        IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        var service = new RegistrationService(registrations, courses, mapper, NullLogger<RegistrationService>.Instance);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
            service.RegisterAsync(StudentA, courseId, CancellationToken.None));

        await using var verify = _db.CreateContext();
        var course = await verify.Courses.AsNoTracking().SingleAsync(c => c.Id == courseId);
        var courseRegistrations = await verify.Registrations.AsNoTracking().Where(r => r.CourseId == courseId).ToListAsync();

        // ההרשמה שלנו לא נשמרה
        Assert.DoesNotContain(courseRegistrations, r => r.UserId == StudentA);
        // רק ההרשמה של המתחרה קיימת, והמכסה עלתה רק בגללה (1 ולא 2)
        Assert.Single(courseRegistrations);
        Assert.Equal(1, course.CurrentCount);
    }

    // Decorator לבדיקה בלבד: מעביר הכל ל-CourseRepository האמיתי,
    // ומיד אחרי GetForUpdateAsync מבצע הרשמה מתחרה דרך DbContext נפרד (כמו בקשה מקבילה של משתמש אחר)
    private sealed class CompetingRegistrationCourseRepository : ICourseRepository
    {
        private readonly ICourseRepository _inner;
        private readonly PostgresFixture _db;
        private readonly int _competitorId;

        public CompetingRegistrationCourseRepository(ICourseRepository inner, PostgresFixture db, int competitorId)
        {
            _inner = inner;
            _db = db;
            _competitorId = competitorId;
        }

        public async Task<Course?> GetForUpdateAsync(int id, CancellationToken ct)
        {
            var course = await _inner.GetForUpdateAsync(id, ct);

            await using var other = _db.CreateContext();
            var sameCourse = await other.Courses.SingleAsync(c => c.Id == id, ct);
            sameCourse.CurrentCount++;
            other.Registrations.Add(new Registration { UserId = _competitorId, CourseId = id, RegisteredAt = DateTime.UtcNow });
            await other.SaveChangesAsync(ct);

            return course;
        }

        public Task<PagedResult<Course>> GetPagedAsync(CourseQueryParameters query, CancellationToken ct) => _inner.GetPagedAsync(query, ct);
        public Task<Course?> GetByIdAsync(int id, CancellationToken ct) => _inner.GetByIdAsync(id, ct);
        public Task<bool> HasRegistrationsAsync(int id, CancellationToken ct) => _inner.HasRegistrationsAsync(id, ct);
        public void Add(Course course) => _inner.Add(course);
        public void Remove(Course course) => _inner.Remove(course);
        public Task SaveChangesAsync(CancellationToken ct) => _inner.SaveChangesAsync(ct);
    }
}
