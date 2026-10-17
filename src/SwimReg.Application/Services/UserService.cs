using AutoMapper;
using SwimReg.Application.DTOs.Auth;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Interfaces.Services;
using SwimReg.Domain.Enums;
using SwimReg.Domain.Exceptions;

namespace SwimReg.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IMapper _mapper;

    public UserService(IUserRepository users, IMapper mapper)
    {
        _users = users;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<InstructorResponseDto>> GetInstructorsAsync(CancellationToken ct)
    {
        var instructors = await _users.GetInstructorsAsync(ct);
        return _mapper.Map<List<InstructorResponseDto>>(instructors);
    }

    public async Task<UserResponseDto> GetByIdAsync(int id, int userId, Role role, CancellationToken ct)
    {
        // משתמש רואה רק את עצמו, Admin רואה כל משתמש
        if (role != Role.Admin && id != userId)
            throw new ForbiddenException("אין הרשאה לצפות במשתמש אחר");

        var user = await _users.GetByIdAsync(id, ct) ?? throw new NotFoundException($"משתמש {id} לא נמצא");
        return _mapper.Map<UserResponseDto>(user);
    }
}
