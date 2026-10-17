using Microsoft.AspNetCore.Mvc;
using SwimReg.API.Errors;
using SwimReg.Application.DTOs.Auth;
using SwimReg.Application.Interfaces.Services;

namespace SwimReg.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // הרשמה פתוחה: יוצרת תמיד משתמש Student
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
    {
        var user = await _auth.RegisterAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(dto, ct);
        if (result is null)
            return Unauthorized(ErrorResponse.Create(HttpContext, StatusCodes.Status401Unauthorized, "אימייל או סיסמה שגויים"));

        return Ok(result);
    }
}
