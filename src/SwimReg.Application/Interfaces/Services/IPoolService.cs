using SwimReg.Application.DTOs.Pools;

namespace SwimReg.Application.Interfaces.Services;

public interface IPoolService
{
    Task<IReadOnlyList<PoolResponseDto>> GetAllAsync(CancellationToken ct);
    Task<PoolResponseDto> GetByIdAsync(int id, CancellationToken ct);
    Task<PoolResponseDto> CreateAsync(PoolRequestDto dto, CancellationToken ct);
    Task UpdateAsync(int id, PoolRequestDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
