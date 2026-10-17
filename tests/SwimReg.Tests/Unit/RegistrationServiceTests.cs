using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Mapping;
using SwimReg.Application.Services;
using SwimReg.Domain.Entities;
using SwimReg.Domain.Enums;
using SwimReg.Domain.Exceptions;

namespace SwimReg.Tests.Unit;

// בדיקות יחידה לשכבת Application: ה-Repositories וה-Logger הם Mock, ה-Mapper אמיתי.
public class RegistrationServiceTests
{
    private const int StudentId = 4;
    private const int CourseId = 1;

    private readonly Mock<IRegistrationRepository> _registrations = new();
    private readonly Mock<ICourseRepository> _courses = new();
    private readonly Mock<ILogger<RegistrationService>> _logger = new();
    private readonly RegistrationService _service;

    public RegistrationServiceTests()
    {
        IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _service = new RegistrationService(_registrations.Object, _courses.Object, mapper, _logger.Object);
    }

    private static Course NewCourse(int current, int max) =>
        new() { Id = CourseId, Name = "שחייה למתחילים", CurrentCount = current, MaxParticipants = max, InstructorId = 2 };

    private void SetupCourse(Course? course) =>
        _courses.Setup(r => r.GetForUpdateAsync(CourseId, It.IsAny<CancellationToken>())).ReturnsAsync(course);

    [Fact]
    public async Task Register_WhenCourseHasSpace_CreatesRegistration()
    {
        var course = NewCourse(current: 1, max: 10);
        SetupCourse(course);

        var result = await _service.RegisterAsync(StudentId, CourseId, CancellationToken.None);

        Assert.Equal(2, course.CurrentCount);
        Assert.Equal(StudentId, result.UserId);
        Assert.Equal(CourseId, result.CourseId);
        _registrations.Verify(r => r.Add(It.Is<Registration>(x => x.UserId == StudentId && x.CourseId == CourseId)), Times.Once);
        // קריאה אחת בלבד לשמירה: עדכון הקורס וההרשמה באותה טרנזקציה
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WhenCourseIsFull_ThrowsConflict()
    {
        SetupCourse(NewCourse(current: 1, max: 1));

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(StudentId, CourseId, CancellationToken.None));

        _registrations.Verify(r => r.Add(It.IsAny<Registration>()), Times.Never);
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyLogged(LogLevel.Warning);
    }

    [Fact]
    public async Task Register_WhenAlreadyRegistered_ThrowsConflict()
    {
        var course = NewCourse(current: 1, max: 10);
        SetupCourse(course);
        _registrations.Setup(r => r.ExistsAsync(StudentId, CourseId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(StudentId, CourseId, CancellationToken.None));

        Assert.Equal(1, course.CurrentCount);
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenCourseNotFound_ThrowsNotFound()
    {
        SetupCourse(null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RegisterAsync(StudentId, CourseId, CancellationToken.None));
    }

    [Fact]
    public async Task Register_WhenConcurrencyConflict_ThrowsConcurrencyConflict()
    {
        // ה-Mock מדמה את ה-Repository: הוא כבר תרגם את DbUpdateConcurrencyException ל-ConcurrencyConflictException
        SetupCourse(NewCourse(current: 0, max: 1));
        _registrations.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("conflict"));

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => _service.RegisterAsync(StudentId, CourseId, CancellationToken.None));

        VerifyLogged(LogLevel.Warning);
    }

    [Fact]
    public async Task Cancel_WhenOwner_DeletesAndDecrementsCount()
    {
        var course = NewCourse(current: 3, max: 10);
        var registration = new Registration { Id = 7, UserId = StudentId, CourseId = CourseId, Course = course };
        _registrations.Setup(r => r.GetForUpdateAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(registration);

        await _service.CancelAsync(7, StudentId, Role.Student, CancellationToken.None);

        Assert.Equal(2, course.CurrentCount);
        _registrations.Verify(r => r.Remove(registration), Times.Once);
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WhenNotOwner_ThrowsForbidden()
    {
        var course = NewCourse(current: 3, max: 10);
        var registration = new Registration { Id = 7, UserId = 5, CourseId = CourseId, Course = course };
        _registrations.Setup(r => r.GetForUpdateAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(registration);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.CancelAsync(7, StudentId, Role.Student, CancellationToken.None));

        Assert.Equal(3, course.CurrentCount);
        _registrations.Verify(r => r.Remove(It.IsAny<Registration>()), Times.Never);
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetStudents_WhenOtherInstructor_ThrowsForbidden()
    {
        // מדריך 2 מבקש את התלמידים של מדריך 3
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.GetStudentsForInstructorAsync(instructorId: 3, userId: 2, Role.Instructor, CancellationToken.None));

        _registrations.Verify(r => r.GetByInstructorAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void VerifyLogged(LogLevel level) =>
        _logger.Verify(l => l.Log(
            level,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
}
