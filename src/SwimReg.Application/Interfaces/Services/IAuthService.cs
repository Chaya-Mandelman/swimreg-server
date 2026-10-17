using SwimReg.Application.DTOs.Auth;

namespace SwimReg.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct);
    // מחזיר null כשהאימייל או הסיסמה שגויים (→ 401)
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct);
}
