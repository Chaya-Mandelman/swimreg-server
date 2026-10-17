using System.ComponentModel.DataAnnotations;

namespace SwimReg.Application.DTOs.Registrations;

public class CreateRegistrationDto
{
    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }
}
