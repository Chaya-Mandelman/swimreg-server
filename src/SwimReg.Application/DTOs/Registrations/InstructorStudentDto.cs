namespace SwimReg.Application.DTOs.Registrations;

public class InstructorStudentDto
{
    public int RegistrationId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
