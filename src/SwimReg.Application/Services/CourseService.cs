using AutoMapper;
using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Courses;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Entities;
using SwimReg.Domain.Enums;
using SwimReg.Domain.Exceptions;

namespace SwimReg.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly IPoolRepository _pools;
    private readonly IUserRepository _users;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository courses, IPoolRepository pools, IUserRepository users, IMapper mapper)
    {
        _courses = courses;
        _pools = pools;
        _users = users;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseResponseDto>> GetPagedAsync(CourseQueryParameters query, CancellationToken ct)
    {
        var page = await _courses.GetPagedAsync(query, ct);
        return new PagedResult<CourseResponseDto>
        {
            Items = _mapper.Map<List<CourseResponseDto>>(page.Items),
            TotalCount = page.TotalCount,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }

    public async Task<CourseResponseDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var course = await _courses.GetByIdAsync(id, ct) ?? throw new NotFoundException($"קורס {id} לא נמצא");
        return _mapper.Map<CourseResponseDto>(course);
    }

    public async Task<CourseResponseDto> CreateAsync(CourseRequestDto dto, CancellationToken ct)
    {
        await ValidateReferencesAsync(dto, ct);

        var course = _mapper.Map<Course>(dto);
        course.CurrentCount = 0;
        _courses.Add(course);
        await _courses.SaveChangesAsync(ct);

        // טעינה מחדש כדי להחזיר גם את שם הבריכה ושם המדריך
        return await GetByIdAsync(course.Id, ct);
    }

    public async Task UpdateAsync(int id, CourseRequestDto dto, CancellationToken ct)
    {
        var course = await _courses.GetForUpdateAsync(id, ct) ?? throw new NotFoundException($"קורס {id} לא נמצא");

        await ValidateReferencesAsync(dto, ct);

        if (dto.MaxParticipants < course.CurrentCount)
            throw new BusinessValidationException(
                $"אי אפשר להוריד את מספר המקומות ל-{dto.MaxParticipants}: כבר רשומים {course.CurrentCount} מתאמנים");

        _mapper.Map(dto, course);

        // אם מישהו נרשם בין הטעינה לשמירה, ה-xmin השתנה וה-Repository יזרוק ConcurrencyConflict → 409
        await _courses.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var course = await _courses.GetForUpdateAsync(id, ct) ?? throw new NotFoundException($"קורס {id} לא נמצא");

        if (await _courses.HasRegistrationsAsync(id, ct))
            throw new ConflictException("לא ניתן למחוק קורס שיש בו הרשמות");

        _courses.Remove(course);
        await _courses.SaveChangesAsync(ct);
    }

    // ולידציה עסקית: הבריכה קיימת, וה-InstructorId שייך למשתמש עם תפקיד Instructor
    private async Task ValidateReferencesAsync(CourseRequestDto dto, CancellationToken ct)
    {
        if (!await _pools.ExistsAsync(dto.PoolId, ct))
            throw new BusinessValidationException($"בריכה {dto.PoolId} לא קיימת");

        var instructor = await _users.GetByIdAsync(dto.InstructorId, ct);
        if (instructor is null || instructor.Role != Role.Instructor)
            throw new BusinessValidationException($"משתמש {dto.InstructorId} אינו מדריך");
    }
}
