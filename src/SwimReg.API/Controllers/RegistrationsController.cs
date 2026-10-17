using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimReg.API.Errors;
using SwimReg.API.Extensions;
using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Registrations;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Enums;

namespace SwimReg.API.Controllers;

[ApiController]
[Route("api/registrations")]
[Authorize]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrations;

    public RegistrationsController(IRegistrationService registrations) => _registrations = registrations;

    // ה-UserId נלקח מה-token, אף פעם לא מהגוף של הבקשה
    [HttpPost]
    [Authorize(Roles = nameof(Role.Student))]
    [ProducesResponseType(typeof(RegistrationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrationResponseDto>> Create([FromBody] CreateRegistrationDto dto, CancellationToken ct)
    {
        var created = await _registrations.RegisterAsync(User.GetUserId(), dto.CourseId, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    [Authorize(Roles = $"{nameof(Role.Student)},{nameof(Role.Admin)}")]
    [ProducesResponseType(typeof(PagedResult<RegistrationResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RegistrationResponseDto>>> GetAll([FromQuery] RegistrationQueryParameters query, CancellationToken ct) =>
        Ok(await _registrations.GetPagedAsync(User.GetUserId(), User.GetRole(), query, ct));

    // הבעלים או Admin. הבדיקה עצמה ב-Service
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RegistrationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationResponseDto>> GetById([FromRoute] int id, CancellationToken ct) =>
        Ok(await _registrations.GetByIdAsync(id, User.GetUserId(), User.GetRole(), ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _registrations.CancelAsync(id, User.GetUserId(), User.GetRole(), ct);
        return NoContent();
    }
}
