using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Courses;

namespace SwimReg.Application.Interfaces.Services;

public interface ICourseService
{
    Task<PagedResult<CourseResponseDto>> GetPagedAsync(CourseQueryParameters query, CancellationToken ct);
    Task<CourseResponseDto> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CourseRequestDto dto, CancellationToken ct);
    Task UpdateAsync(int id, CourseRequestDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
