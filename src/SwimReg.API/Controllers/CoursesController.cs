using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimReg.API.Errors;
using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Courses;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Enums;

namespace SwimReg.API.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courses;

    public CoursesController(ICourseService courses) => _courses = courses;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CourseResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CourseResponseDto>>> GetAll([FromQuery] CourseQueryParameters query, CancellationToken ct) =>
        Ok(await _courses.GetPagedAsync(query, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseResponseDto>> GetById([FromRoute] int id, CancellationToken ct) =>
        Ok(await _courses.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CourseResponseDto>> Create([FromBody] CourseRequestDto dto, CancellationToken ct)
    {
        var created = await _courses.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CourseRequestDto dto, CancellationToken ct)
    {
        await _courses.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _courses.DeleteAsync(id, ct);
        return NoContent();
    }
}
