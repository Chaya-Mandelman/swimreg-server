using SwimReg.Application.Common;
using SwimReg.Application.DTOs.Registrations;
using SwimReg.Domain.Enums;

namespace SwimReg.Application.Interfaces.Services;

// userId ו-role מגיעים תמיד מה-token (claims), לא מהלקוח
public interface IRegistrationService
{
    Task<RegistrationResponseDto> RegisterAsync(int userId, int courseId, CancellationToken ct);
    Task<PagedResult<RegistrationResponseDto>> GetPagedAsync(int userId, Role role, RegistrationQueryParameters query, CancellationToken ct);
    Task<RegistrationResponseDto> GetByIdAsync(int id, int userId, Role role, CancellationToken ct);
    Task CancelAsync(int id, int userId, Role role, CancellationToken ct);
    Task<IReadOnlyList<InstructorStudentDto>> GetStudentsForInstructorAsync(int instructorId, int userId, Role role, CancellationToken ct);
}
