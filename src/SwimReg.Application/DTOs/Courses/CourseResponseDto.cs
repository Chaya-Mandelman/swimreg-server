namespace SwimReg.Application.DTOs.Courses;

public class CourseResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentCount { get; set; }
    public int PoolId { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
}
