using System.ComponentModel.DataAnnotations;
using SwimReg.Domain.Enums;

namespace SwimReg.Application.DTOs.Courses;

// משמש גם ליצירה (POST) וגם לעדכון (PUT)
public class CourseRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    // nullable כדי ש-[Required] באמת יזהה שדה חסר (אחרת יתקבל ערך ברירת המחדל 0)
    [Required]
    public CourseLevel? Level { get; set; }

    [Required]
    public DayOfWeek? DayOfWeek { get; set; }

    [Required]
    public TimeOnly? StartTime { get; set; }

    [Range(1, 30)]
    public int MaxParticipants { get; set; }

    [Range(1, int.MaxValue)]
    public int PoolId { get; set; }

    [Range(1, int.MaxValue)]
    public int InstructorId { get; set; }
}
