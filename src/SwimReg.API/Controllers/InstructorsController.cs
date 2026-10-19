using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimReg.API.Errors;
using SwimReg.API.Extensions;
using SwimReg.Application.DTOs.Auth;
using SwimReg.Application.DTOs.Registrations;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Enums;

namespace SwimReg.API.Controllers;

[ApiController]
[Route("api/instructors")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
public class InstructorsController : ControllerBase
{
    private readonly IRegistrationService _registrations;
    private readonly IUserService _users;

    public InstructorsController(IRegistrationService registrations, IUserService users)
    {
        _registrations = registrations;
        _users = users;
    }

    // רשימת המדריכים, לבחירת מדריך בטופס הקורס
    [HttpGet]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(IReadOnlyList<InstructorResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InstructorResponseDto>>> GetAll(CancellationToken ct) =>
        Ok(await _users.GetInstructorsAsync(ct));

    // מדריך רק עם ה-id שלו (אחרת 403). הבדיקה ב-Service
    [HttpGet("{id:int}/students")]
    [Authorize(Roles = $"{nameof(Role.Instructor)},{nameof(Role.Admin)}")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(IReadOnlyList<InstructorStudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InstructorStudentDto>>> GetStudents([FromRoute] int id, CancellationToken ct) =>
        Ok(await _registrations.GetStudentsForInstructorAsync(id, User.GetUserId(), User.GetRole(), ct));
}
