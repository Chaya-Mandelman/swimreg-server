using System.ComponentModel.DataAnnotations;

namespace SwimReg.Application.Common;

// פרמטרי דפדוף משותפים: Page ≥ 1, PageSize בין 1 ל-50
public class PagingParameters
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
