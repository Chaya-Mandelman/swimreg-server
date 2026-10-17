using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimReg.API.Errors;
using SwimReg.Application.DTOs.Pools;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Enums;

namespace SwimReg.API.Controllers;

[ApiController]
[Route("api/pools")]
[Authorize]
public class PoolsController : ControllerBase
{
    private readonly IPoolService _pools;

    public PoolsController(IPoolService pools) => _pools = pools;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PoolResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PoolResponseDto>>> GetAll(CancellationToken ct) =>
        Ok(await _pools.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PoolResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PoolResponseDto>> GetById([FromRoute] int id, CancellationToken ct) =>
        Ok(await _pools.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(typeof(PoolResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PoolResponseDto>> Create([FromBody] PoolRequestDto dto, CancellationToken ct)
    {
        var created = await _pools.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PoolRequestDto dto, CancellationToken ct)
    {
        await _pools.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _pools.DeleteAsync(id, ct);
        return NoContent();
    }
}
