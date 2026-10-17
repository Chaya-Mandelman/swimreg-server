using Microsoft.EntityFrameworkCore;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Domain.Entities;
using SwimReg.Infrastructure.Persistence;

namespace SwimReg.Infrastructure.Repositories;

public class PoolRepository : IPoolRepository
{
    private readonly SwimRegDbContext _context;

    public PoolRepository(SwimRegDbContext context) => _context = context;

    public async Task<IReadOnlyList<Pool>> GetAllAsync(CancellationToken ct) =>
        await _context.Pools.AsNoTracking().OrderBy(p => p.Id).ToListAsync(ct);

    public Task<Pool?> GetByIdAsync(int id, CancellationToken ct) =>
        _context.Pools.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Pool?> GetForUpdateAsync(int id, CancellationToken ct) =>
        _context.Pools.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        _context.Pools.AnyAsync(p => p.Id == id, ct);

    public Task<bool> HasCoursesAsync(int id, CancellationToken ct) =>
        _context.Courses.AnyAsync(c => c.PoolId == id, ct);

    public void Add(Pool pool) => _context.Pools.Add(pool);

    public void Remove(Pool pool) => _context.Pools.Remove(pool);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesMappedAsync(ct);
}
