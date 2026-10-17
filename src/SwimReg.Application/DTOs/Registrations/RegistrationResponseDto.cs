namespace SwimReg.Application.DTOs.Registrations;

public class RegistrationResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string PoolName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
