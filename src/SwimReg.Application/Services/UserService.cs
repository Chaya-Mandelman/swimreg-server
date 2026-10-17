using AutoMapper;
using SwimReg.Application.DTOs.Auth;
using SwimReg.Application.Interfaces.Repositories;
using SwimReg.Application.Interfaces.Services;

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
}
