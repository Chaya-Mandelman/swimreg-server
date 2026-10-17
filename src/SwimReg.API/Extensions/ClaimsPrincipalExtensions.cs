using System.Security.Claims;
using SwimReg.Domain.Enums;

namespace SwimReg.API.Extensions;

// קריאת זהות המשתמש מה-token. ה-Controllers מעבירים את הערכים ל-Services לבדיקות בעלות
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("Missing user id claim"));

    public static Role GetRole(this ClaimsPrincipal user) =>
        Enum.Parse<Role>(user.FindFirstValue(ClaimTypes.Role)
                         ?? throw new UnauthorizedAccessException("Missing role claim"));
}
