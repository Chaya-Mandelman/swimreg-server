namespace SwimReg.Domain.Entities;

public class Registration
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public DateTime RegisteredAt { get; set; }
}
