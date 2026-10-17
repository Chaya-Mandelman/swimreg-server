using System.ComponentModel.DataAnnotations;

namespace SwimReg.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "אימייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "סיסמה היא שדה חובה")]
    public string Password { get; set; } = string.Empty;
}
