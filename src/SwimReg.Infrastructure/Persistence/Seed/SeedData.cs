using SwimReg.Domain.Entities;
using SwimReg.Domain.Enums;

namespace SwimReg.Infrastructure.Persistence.Seed;

// נתוני דמו שנכנסים דרך HasData במיגרציה.
// הכל קבוע (Id, hash, תאריכים), כי HasData משווה בין מיגרציות וערך משתנה ייצור מיגרציה חדשה בכל פעם.
internal static class SeedData
{
    // hash של BCrypt שחושב מראש: Admin123! / Instructor123! / Student123!
    private const string AdminHash = "$2a$11$NJPE9ZQnaC8qKWCRjlbxIO94jAtlu726auVvB5YYKf4oWkSPJsvb6";
    private const string InstructorHash = "$2a$11$E/DSeS0ZC5TtghA5BMvGGOdXD9fm5AuyNgB0uHIPISD2xtAn/tlt2";
    private const string StudentHash = "$2a$11$OgQlFlNG1zTNgxgyDYjYs.Za3Mh4cEghzjC3wSogeSTBg0Jv9M7S.";

    public static readonly User[] Users =
    {
        new() { Id = 1, FullName = "מנהלת המערכת", Email = "admin@swimreg.com", PasswordHash = AdminHash, Role = Role.Admin },
        new() { Id = 2, FullName = "דנה כהן", Email = "dana@swimreg.com", PasswordHash = InstructorHash, Role = Role.Instructor },
        new() { Id = 3, FullName = "יוסי לוי", Email = "yossi@swimreg.com", PasswordHash = InstructorHash, Role = Role.Instructor },
        new() { Id = 4, FullName = "נועה פרץ", Email = "noa@swimreg.com", PasswordHash = StudentHash, Role = Role.Student },
        new() { Id = 5, FullName = "איתי מזרחי", Email = "itay@swimreg.com", PasswordHash = StudentHash, Role = Role.Student }
    };

    public static readonly Pool[] Pools =
    {
        new() { Id = 1, Name = "בריכה עירונית מרכז", Address = "רחוב הים 10, תל אביב" },
        new() { Id = 2, Name = "בריכת הצפון", Address = "שדרות הנשיא 5, חיפה" }
    };

    // CurrentCount של כל קורס = בדיוק מספר ההרשמות שלו ב-Registrations למטה
    public static readonly Course[] Courses =
    {
        new() { Id = 1, Name = "שחייה למתחילים", Level = CourseLevel.Beginner, DayOfWeek = DayOfWeek.Sunday,
                StartTime = new TimeOnly(17, 0), MaxParticipants = 10, CurrentCount = 1, PoolId = 1, InstructorId = 2 },
        new() { Id = 2, Name = "שחייה מתקדמים-בינוני", Level = CourseLevel.Intermediate, DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(18, 30), MaxParticipants = 8, CurrentCount = 0, PoolId = 1, InstructorId = 2 },
        new() { Id = 3, Name = "אימון מתקדמים", Level = CourseLevel.Advanced, DayOfWeek = DayOfWeek.Wednesday,
                StartTime = new TimeOnly(19, 0), MaxParticipants = 12, CurrentCount = 1, PoolId = 2, InstructorId = 3 },
        // קורס עם מקום אחד בלבד שעדיין פנוי: הרשמה אחת תמלא אותו, והשנייה תקבל 409
        new() { Id = 4, Name = "שיעור פרטי", Level = CourseLevel.Beginner, DayOfWeek = DayOfWeek.Thursday,
                StartTime = new TimeOnly(16, 0), MaxParticipants = 1, CurrentCount = 0, PoolId = 2, InstructorId = 3 }
    };

    public static readonly Registration[] Registrations =
    {
        new() { Id = 1, UserId = 4, CourseId = 1, RegisteredAt = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc) },
        new() { Id = 2, UserId = 5, CourseId = 3, RegisteredAt = new DateTime(2026, 9, 2, 9, 30, 0, DateTimeKind.Utc) }
    };
}
