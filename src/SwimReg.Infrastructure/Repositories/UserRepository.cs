using Microsoft.EntityFrameworkCore;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Domain.Entities;
using SwimReg.Domain.Enums;
using SwimReg.Infrastructure.Persistence;

namespace SwimReg.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SwimRegDbContext _context;

    public UserRepository(SwimRegDbContext context) => _context = context;

    public Task<User?> GetByIdAsync(int id, CancellationToken ct) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
        _context.Users.AnyAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<User>> GetInstructorsAsync(CancellationToken ct) =>
        await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == Role.Instructor)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct)
    {
        _context.Users.Add(user);
        await _context.SaveChangesMappedAsync(ct);
    }
}
