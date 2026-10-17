using SwimReg.Domain.Entities;

namespace SwimReg.Application.Interfaces.Repositories;

public interface IPoolRepository
{
    Task<IReadOnlyList<Pool>> GetAllAsync(CancellationToken ct);
    Task<Pool?> GetByIdAsync(int id, CancellationToken ct);
    // טעינה עם tracking לצורך עדכון או מחיקה
    Task<Pool?> GetForUpdateAsync(int id, CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task<bool> HasCoursesAsync(int id, CancellationToken ct);
    void Add(Pool pool);
    void Remove(Pool pool);
    Task SaveChangesAsync(CancellationToken ct);
}
