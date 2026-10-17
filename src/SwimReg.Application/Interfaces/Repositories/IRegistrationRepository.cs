using SwimReg.Application.Common;
using SwimReg.Domain.Entities;

namespace SwimReg.Application.Interfaces.Repositories;

public interface IRegistrationRepository
{
    Task<bool> ExistsAsync(int userId, int courseId, CancellationToken ct);
    Task<Registration?> GetByIdAsync(int id, CancellationToken ct);
    // טעינה עם tracking כולל הקורס, כדי למחוק ולהוריד את CurrentCount באותה שמירה
    Task<Registration?> GetForUpdateAsync(int id, CancellationToken ct);
    // userId == null → כל ההרשמות (Admin)
    Task<PagedResult<Registration>> GetPagedAsync(int? userId, int? courseId, int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<Registration>> GetByInstructorAsync(int instructorId, CancellationToken ct);
    void Add(Registration registration);
    void Remove(Registration registration);
    // שומר את כל השינויים של ה-DbContext (גם של הקורס) בטרנזקציה אחת
    Task SaveChangesAsync(CancellationToken ct);
}
