using System.ComponentModel.DataAnnotations;

namespace SwimReg.Application.DTOs.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "שם מלא הוא שדה חובה")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "שם מלא צריך להכיל בין 2 ל-100 תווים")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "אימייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
    [StringLength(200, ErrorMessage = "אימייל יכול להכיל עד 200 תווים")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "סיסמה היא שדה חובה")]
    [MinLength(6, ErrorMessage = "הסיסמה צריכה להכיל לפחות 6 תווים")]
    [StringLength(100, ErrorMessage = "הסיסמה יכולה להכיל עד 100 תווים")]
    public string Password { get; set; } = string.Empty;
}
