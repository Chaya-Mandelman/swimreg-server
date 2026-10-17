using SwimReg.Application.DTOs.Auth;
using SwimReg.Domain.Enums;

namespace SwimReg.Application.Interfaces.Services;

public interface IUserService
{
    Task<IReadOnlyList<InstructorResponseDto>> GetInstructorsAsync(CancellationToken ct);
    Task<UserResponseDto> GetByIdAsync(int id, int userId, Role role, CancellationToken ct);
}
