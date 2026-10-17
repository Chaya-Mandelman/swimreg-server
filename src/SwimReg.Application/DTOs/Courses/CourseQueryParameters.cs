using System.ComponentModel.DataAnnotations;
using SwimReg.Application.Common;
using SwimReg.Domain.Enums;

namespace SwimReg.Application.DTOs.Courses;

public class CourseQueryParameters : PagingParameters
{
    public CourseLevel? Level { get; set; }

    [Range(1, int.MaxValue)]
    public int? PoolId { get; set; }

    // מיון מותר: name / level / day / startTime (ברירת מחדל: לפי Id)
    [RegularExpression("(?i)^(name|level|day|starttime)$", ErrorMessage = "sortBy must be one of: name, level, day, startTime")]
    public string? SortBy { get; set; }
}
