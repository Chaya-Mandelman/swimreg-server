using AutoMapper;
using SwimReg.Application.DTOs.Pools;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Entities;
using SwimReg.Domain.Exceptions;

namespace SwimReg.Application.Services;

public class PoolService : IPoolService
{
    private readonly IPoolRepository _pools;
    private readonly IMapper _mapper;

    public PoolService(IPoolRepository pools, IMapper mapper)
    {
        _pools = pools;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PoolResponseDto>> GetAllAsync(CancellationToken ct)
    {
        var pools = await _pools.GetAllAsync(ct);
        return _mapper.Map<List<PoolResponseDto>>(pools);
    }

    public async Task<PoolResponseDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var pool = await _pools.GetByIdAsync(id, ct) ?? throw new NotFoundException($"בריכה {id} לא נמצאה");
        return _mapper.Map<PoolResponseDto>(pool);
    }

    public async Task<PoolResponseDto> CreateAsync(PoolRequestDto dto, CancellationToken ct)
    {
        var pool = _mapper.Map<Pool>(dto);
        _pools.Add(pool);
        await _pools.SaveChangesAsync(ct);
        return _mapper.Map<PoolResponseDto>(pool);
    }

    public async Task UpdateAsync(int id, PoolRequestDto dto, CancellationToken ct)
    {
        var pool = await _pools.GetForUpdateAsync(id, ct) ?? throw new NotFoundException($"בריכה {id} לא נמצאה");
        _mapper.Map(dto, pool);
        await _pools.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var pool = await _pools.GetForUpdateAsync(id, ct) ?? throw new NotFoundException($"בריכה {id} לא נמצאה");

        // בדיקה עסקית מראש. אם בכל זאת נוצר קורס במקביל, ה-Repository יתרגם את שגיאת ה-FK ל-409
        if (await _pools.HasCoursesAsync(id, ct))
            throw new ConflictException("לא ניתן למחוק בריכה שיש בה קורסים");

        _pools.Remove(pool);
        await _pools.SaveChangesAsync(ct);
    }
}
