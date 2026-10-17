using Microsoft.EntityFrameworkCore;
using SwimReg.Application.Common;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Domain.Entities;
using SwimReg.Infrastructure.Persistence;

namespace SwimReg.Infrastructure.Repositories;

public class RegistrationRepository : IRegistrationRepository
{
    private readonly SwimRegDbContext _context;

    public RegistrationRepository(SwimRegDbContext context) => _context = context;

    public Task<bool> ExistsAsync(int userId, int courseId, CancellationToken ct) =>
        _context.Registrations.AnyAsync(r => r.UserId == userId && r.CourseId == courseId, ct);

    public Task<Registration?> GetByIdAsync(int id, CancellationToken ct) =>
        _context.Registrations
            .AsNoTracking()
            .Include(r => r.Course).ThenInclude(c => c!.Pool)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    // עם tracking: גם ההרשמה וגם הקורס, כדי למחוק ולעדכן את המכסה בשמירה אחת
    public Task<Registration?> GetForUpdateAsync(int id, CancellationToken ct) =>
        _context.Registrations
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<PagedResult<Registration>> GetPagedAsync(int? userId, int? courseId, int page, int pageSize, CancellationToken ct)
    {
        IQueryable<Registration> query = _context.Registrations
            .AsNoTracking()
            .Include(r => r.Course).ThenInclude(c => c!.Pool)
            .Include(r => r.User);

        if (userId.HasValue)
            query = query.Where(r => r.UserId == userId.Value);
        if (courseId.HasValue)
            query = query.Where(r => r.CourseId == courseId.Value);

        query = query.OrderByDescending(r => r.RegisteredAt).ThenBy(r => r.Id);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<Registration> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    // שאילתה אחת עם JOIN, בלי לולאה על הקורסים: אין בעיית N+1
    public async Task<IReadOnlyList<Registration>> GetByInstructorAsync(int instructorId, CancellationToken ct) =>
        await _context.Registrations
            .AsNoTracking()
            .Include(r => r.Course)
            .Include(r => r.User)
            .Where(r => r.Course!.InstructorId == instructorId)
            .OrderBy(r => r.CourseId).ThenBy(r => r.User!.FullName)
            .ToListAsync(ct);

    public void Add(Registration registration) => _context.Registrations.Add(registration);

    public void Remove(Registration registration) => _context.Registrations.Remove(registration);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesMappedAsync(ct);
}
