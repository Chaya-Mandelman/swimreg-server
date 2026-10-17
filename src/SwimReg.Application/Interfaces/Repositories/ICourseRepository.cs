using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Courses;
using SwimReg.Domain.Entities;

namespace SwimReg.Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<PagedResult<Course>> GetPagedAsync(CourseQueryParameters query, CancellationToken ct);
    Task<Course?> GetByIdAsync(int id, CancellationToken ct);
    // טעינה עם tracking: EF זוכר את ה-Version (xmin) שנטען ובודק אותו בשמירה
    Task<Course?> GetForUpdateAsync(int id, CancellationToken ct);
    Task<bool> HasRegistrationsAsync(int id, CancellationToken ct);
    void Add(Course course);
    void Remove(Course course);
    Task SaveChangesAsync(CancellationToken ct);
}
