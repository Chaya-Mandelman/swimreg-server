using SwimReg.Domain.Entities;

namespace SwimReg.Application.Interfaces.Security;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
