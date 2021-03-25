using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Models.Profiles
{
    public class CoursesProfile: Profile
    {
        public CoursesProfile()
        {
            CreateMap<Course, CourseDto>();
        }
    }
}
