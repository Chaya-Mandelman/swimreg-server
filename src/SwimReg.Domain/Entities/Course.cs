using SwimReg.Domain.Enums;

namespace SwimReg.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CourseLevel Level { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }

    // המשאב המוגבל: מספר המקומות בקורס
    public int MaxParticipants { get; set; }
    public int CurrentCount { get; set; }

    public int PoolId { get; set; }
    public Pool? Pool { get; set; }

    public int InstructorId { get; set; }
    public User? Instructor { get; set; }

    // Concurrency token: ממופה לעמודת המערכת xmin של PostgreSQL (ראה CourseConfiguration)
    public uint Version { get; set; }

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
