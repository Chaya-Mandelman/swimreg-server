using SwimReg.Domain.Entities;

namespace SwimReg.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<User>> GetInstructorsAsync(CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}
