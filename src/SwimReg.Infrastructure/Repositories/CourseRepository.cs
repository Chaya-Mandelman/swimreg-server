using Microsoft.EntityFrameworkCore;
using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Courses;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Domain.Entities;
using SwimReg.Infrastructure.Persistence;

namespace SwimReg.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly SwimRegDbContext _context;

    public CourseRepository(SwimRegDbContext context) => _context = context;

    public async Task<PagedResult<Course>> GetPagedAsync(CourseQueryParameters query, CancellationToken ct)
    {
        // Pagination אמיתי: Where → OrderBy → Count → Skip/Take, הכל רץ ב-SQL
        IQueryable<Course> courses = _context.Courses
            .AsNoTracking()
            .Include(c => c.Pool)
            .Include(c => c.Instructor);

        if (query.Level.HasValue)
            courses = courses.Where(c => c.Level == query.Level.Value);
        if (query.PoolId.HasValue)
            courses = courses.Where(c => c.PoolId == query.PoolId.Value);

        courses = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => courses.OrderBy(c => c.Name).ThenBy(c => c.Id),
            "level" => courses.OrderBy(c => c.Level).ThenBy(c => c.Id),
            "day" => courses.OrderBy(c => c.DayOfWeek).ThenBy(c => c.StartTime),
            "starttime" => courses.OrderBy(c => c.StartTime).ThenBy(c => c.Id),
            _ => courses.OrderBy(c => c.Id)
        };

        var total = await courses.CountAsync(ct);
        var items = await courses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Course> { Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize };
    }

    public Task<Course?> GetByIdAsync(int id, CancellationToken ct) =>
        _context.Courses
            .AsNoTracking()
            .Include(c => c.Pool)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    // בלי AsNoTracking: EF צריך לעקוב אחרי הקורס כדי לשלוח UPDATE ... WHERE xmin = @original
    public Task<Course?> GetForUpdateAsync(int id, CancellationToken ct) =>
        _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> HasRegistrationsAsync(int id, CancellationToken ct) =>
        _context.Registrations.AnyAsync(r => r.CourseId == id, ct);

    public void Add(Course course) => _context.Courses.Add(course);

    public void Remove(Course course) => _context.Courses.Remove(course);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesMappedAsync(ct);
}
