using AutoMapper;
using SwimReg.Application.DTOs.Auth;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Interfaces.Security;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Entities;
using SwimReg.Domain.Enums;
using SwimReg.Domain.Exceptions;

namespace SwimReg.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenService tokens, IMapper mapper)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
        _mapper = mapper;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct)
    {
        var email = NormalizeEmail(dto.Email);
        if (await _users.EmailExistsAsync(email, ct))
            throw new ConflictException("האימייל כבר רשום במערכת");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(dto.Password),
            // הרשמה פתוחה יוצרת תמיד Student. אי אפשר לבחור תפקיד מבחוץ
            Role = Role.Student
        };

        await _users.AddAsync(user, ct);
        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(NormalizeEmail(dto.Email), ct);

        // אותה תשובה לאימייל לא קיים ולסיסמה שגויה, כדי לא לחשוף אילו אימיילים קיימים
        if (user is null || !_hasher.Verify(dto.Password, user.PasswordHash))
            return null;

        var (token, expiresAt) = _tokens.CreateToken(user);
        return new AuthResponseDto { Token = token, ExpiresAt = expiresAt, User = _mapper.Map<UserResponseDto>(user) };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
