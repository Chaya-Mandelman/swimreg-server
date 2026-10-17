using SwimReg.Application.Interfaces.Security;

namespace SwimReg.Infrastructure.Security;

// אין state, לכן בטוח כ-Singleton
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
