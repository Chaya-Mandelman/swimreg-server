using SwimReg.Application.DTOs.Auth;

namespace SwimReg.Application.Interfaces.Services;

public interface IUserService
{
    Task<IReadOnlyList<InstructorResponseDto>> GetInstructorsAsync(CancellationToken ct);
}
