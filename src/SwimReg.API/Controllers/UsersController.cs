using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimReg.API.Errors;
using SwimReg.API.Extensions;
using SwimReg.Application.DTOs.Auth;
using SwimReg.Application.Interfaces.Services;

namespace SwimReg.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    // המשתמש עצמו או Admin. הבדיקה ב-Service. זו הכתובת שחוזרת ב-Location אחרי הרשמה
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetById([FromRoute] int id, CancellationToken ct) =>
        Ok(await _users.GetByIdAsync(id, User.GetUserId(), User.GetRole(), ct));
}
