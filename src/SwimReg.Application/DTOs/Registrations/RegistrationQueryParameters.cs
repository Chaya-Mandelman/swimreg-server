using System.ComponentModel.DataAnnotations;
using SwimReg.Application.Common;

namespace SwimReg.Application.DTOs.Registrations;

public class RegistrationQueryParameters : PagingParameters
{
    [Range(1, int.MaxValue)]
    public int? CourseId { get; set; }
}
