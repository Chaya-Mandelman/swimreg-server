using AutoMapper;
using Microsoft.Extensions.Logging;
using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Registrations;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Entities;
using SwimReg.Domain.Enums;
using SwimReg.Domain.Exceptions;

namespace SwimReg.Application.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IRegistrationRepository _registrations;
    private readonly ICourseRepository _courses;
    private readonly IMapper _mapper;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(
        IRegistrationRepository registrations,
        ICourseRepository courses,
        IMapper mapper,
        ILogger<RegistrationService> logger)
    {
        _registrations = registrations;
        _courses = courses;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<RegistrationResponseDto> RegisterAsync(int userId, int courseId, CancellationToken ct)
    {
        // 1. טעינה עם tracking: EF שומר את ה-Version (xmin) המקורי
        var course = await _courses.GetForUpdateAsync(courseId, ct)
            ?? throw new NotFoundException($"קורס {courseId} לא נמצא");

        // 2. בדיקת מכסה
        if (course.CurrentCount >= course.MaxParticipants)
        {
            _logger.LogWarning("Registration rejected: course {CourseId} is full ({Current}/{Max}), user {UserId}",
                courseId, course.CurrentCount, course.MaxParticipants, userId);
            throw new ConflictException("הקורס מלא");
        }

        // 3. הרשמה כפולה
        if (await _registrations.ExistsAsync(userId, courseId, ct))
            throw new ConflictException("כבר נרשמת לקורס הזה");

        // 4. עדכון המכסה והוספת ההרשמה
        course.CurrentCount++;
        var registration = new Registration { UserId = userId, CourseId = courseId, RegisteredAt = DateTime.UtcNow };
        _registrations.Add(registration);

        // 5. קריאה אחת בלבד ל-SaveChanges: UPDATE לקורס (WHERE xmin = מקורי) + INSERT להרשמה, באותה טרנזקציה.
        //    אם מישהו אחר עדכן את הקורס בינתיים, שום דבר לא נשמר: גם לא ההרשמה.
        try
        {
            await _registrations.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            _logger.LogWarning("Concurrency conflict while registering user {UserId} to course {CourseId}", userId, courseId);
            throw;
        }

        _logger.LogInformation("User {UserId} registered to course {CourseId} (registration {RegistrationId})",
            userId, courseId, registration.Id);

        var saved = await _registrations.GetByIdAsync(registration.Id, ct) ?? registration;
        return _mapper.Map<RegistrationResponseDto>(saved);
    }

    public async Task<PagedResult<RegistrationResponseDto>> GetPagedAsync(int userId, Role role, RegistrationQueryParameters query, CancellationToken ct)
    {
        // Student רואה רק את ההרשמות שלו. הסינון נעשה כאן לפי ה-UserId מה-token
        int? ownerFilter = role switch
        {
            Role.Admin => null,
            Role.Student => userId,
            _ => throw new ForbiddenException("אין הרשאה לצפות בהרשמות")
        };

        var page = await _registrations.GetPagedAsync(ownerFilter, query.CourseId, query.Page, query.PageSize, ct);
        return new PagedResult<RegistrationResponseDto>
        {
            Items = _mapper.Map<List<RegistrationResponseDto>>(page.Items),
            TotalCount = page.TotalCount,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }

    public async Task<RegistrationResponseDto> GetByIdAsync(int id, int userId, Role role, CancellationToken ct)
    {
        var registration = await _registrations.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"הרשמה {id} לא נמצאה");

        EnsureOwnerOrAdmin(registration, userId, role);
        return _mapper.Map<RegistrationResponseDto>(registration);
    }

    public async Task CancelAsync(int id, int userId, Role role, CancellationToken ct)
    {
        var registration = await _registrations.GetForUpdateAsync(id, ct)
            ?? throw new NotFoundException($"הרשמה {id} לא נמצאה");

        EnsureOwnerOrAdmin(registration, userId, role);

        // מחיקה + הורדת המכסה באותה שמירה. גם כאן ה-Version של הקורס נבדק
        _registrations.Remove(registration);
        registration.Course!.CurrentCount--;

        try
        {
            await _registrations.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            _logger.LogWarning("Concurrency conflict while cancelling registration {RegistrationId} (course {CourseId})",
                id, registration.CourseId);
            throw;
        }

        _logger.LogInformation("Registration {RegistrationId} cancelled by user {UserId} (course {CourseId})",
            id, userId, registration.CourseId);
    }

    public async Task<IReadOnlyList<InstructorStudentDto>> GetStudentsForInstructorAsync(int instructorId, int userId, Role role, CancellationToken ct)
    {
        // מדריך רואה רק את התלמידים בקורסים שלו
        var allowed = role == Role.Admin || (role == Role.Instructor && instructorId == userId);
        if (!allowed)
            throw new ForbiddenException("אין הרשאה לצפות בתלמידים של מדריך אחר");

        var registrations = await _registrations.GetByInstructorAsync(instructorId, ct);
        return _mapper.Map<List<InstructorStudentDto>>(registrations);
    }

    private static void EnsureOwnerOrAdmin(Registration registration, int userId, Role role)
    {
        if (role != Role.Admin && registration.UserId != userId)
            throw new ForbiddenException("אין הרשאה להרשמה הזו");
    }
}
