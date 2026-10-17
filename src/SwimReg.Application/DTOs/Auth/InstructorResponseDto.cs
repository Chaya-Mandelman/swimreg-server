namespace SwimReg.Application.DTOs.Auth;

// לרשימת המדריכים בטופס הקורס של המנהל
public class InstructorResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
