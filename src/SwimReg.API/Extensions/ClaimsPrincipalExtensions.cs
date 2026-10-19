using System.Security.Claims;
using SwimReg.Domain.Enums;

namespace SwimReg.API.Extensions;

// קריאת זהות המשתמש מה-token. ה-Controllers מעבירים את הערכים ל-Services לבדיקות בעלות
public static class ClaimsPrincipalExtensions
{
    // claim חסר או לא תקין → UnauthorizedAccessException → 401 (ולא FormatException → 500)
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedAccessException("Missing or invalid user id claim");

    public static Role GetRole(this ClaimsPrincipal user) =>
        Enum.TryParse<Role>(user.FindFirstValue(ClaimTypes.Role), out var role)
            ? role
            : throw new UnauthorizedAccessException("Missing or invalid role claim");
}
