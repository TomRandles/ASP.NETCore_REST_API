using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;

namespace CourseLibrary.API.Models.Profiles
{
    public class CoursesProfile: Profile
    {
        public CoursesProfile()
        {
            CreateMap<Course, CourseDto>();
            CreateMap<CourseCreateDto, Course>();
            CreateMap<CourseUpdateDto, Course>();
            CreateMap<Course, CourseUpdateDto>();

        }
    }
}