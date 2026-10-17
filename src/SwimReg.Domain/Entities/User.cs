using SwimReg.Domain.Enums;

namespace SwimReg.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }

    // קורסים שהמשתמש מלמד (רק עבור Instructor)
    public ICollection<Course> InstructedCourses { get; set; } = new List<Course>();

    // הקשר N:M לקורסים עובר דרך Registration
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
