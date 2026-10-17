using AutoMapper;
using SwimReg.Application.DTOs.Auth;
using SwimReg.Application.DTOs.Courses;
using SwimReg.Application.DTOs.Pools;
using SwimReg.Application.DTOs.Registrations;
using SwimReg.Domain.Entities;

namespace SwimReg.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponseDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));
        CreateMap<User, InstructorResponseDto>();

        CreateMap<Pool, PoolResponseDto>();
        CreateMap<PoolRequestDto, Pool>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Courses, o => o.Ignore());

        CreateMap<Course, CourseResponseDto>()
            .ForMember(d => d.Level, o => o.MapFrom(s => s.Level.ToString()))
            .ForMember(d => d.DayOfWeek, o => o.MapFrom(s => s.DayOfWeek.ToString()))
            .ForMember(d => d.PoolName, o => o.MapFrom(s => s.Pool != null ? s.Pool.Name : string.Empty))
            .ForMember(d => d.InstructorName, o => o.MapFrom(s => s.Instructor != null ? s.Instructor.FullName : string.Empty));

        // ה-DTO כבר עבר ולידציה ([Required]), לכן ה-Value בטוח
        CreateMap<CourseRequestDto, Course>()
            .ForMember(d => d.Level, o => o.MapFrom(s => s.Level!.Value))
            .ForMember(d => d.DayOfWeek, o => o.MapFrom(s => s.DayOfWeek!.Value))
            .ForMember(d => d.StartTime, o => o.MapFrom(s => s.StartTime!.Value))
            // שדות שהלקוח לא קובע
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CurrentCount, o => o.Ignore())
            .ForMember(d => d.Version, o => o.Ignore())
            .ForMember(d => d.Pool, o => o.Ignore())
            .ForMember(d => d.Instructor, o => o.Ignore())
            .ForMember(d => d.Registrations, o => o.Ignore());

        CreateMap<Registration, RegistrationResponseDto>()
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course != null ? s.Course.Name : string.Empty))
            .ForMember(d => d.PoolName, o => o.MapFrom(s => s.Course != null && s.Course.Pool != null ? s.Course.Pool.Name : string.Empty));

        CreateMap<Registration, InstructorStudentDto>()
            .ForMember(d => d.RegistrationId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course != null ? s.Course.Name : string.Empty))
            .ForMember(d => d.StudentId, o => o.MapFrom(s => s.UserId))
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.StudentEmail, o => o.MapFrom(s => s.User != null ? s.User.Email : string.Empty));
    }
}
