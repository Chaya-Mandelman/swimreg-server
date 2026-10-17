using System.ComponentModel.DataAnnotations;

namespace SwimReg.Application.DTOs.Pools;

public class PoolRequestDto
{
    [Required(ErrorMessage = "שם הבריכה הוא שדה חובה")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "שם הבריכה צריך להכיל בין 2 ל-100 תווים")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "כתובת היא שדה חובה")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "הכתובת צריכה להכיל בין 2 ל-200 תווים")]
    public string Address { get; set; } = string.Empty;
}
