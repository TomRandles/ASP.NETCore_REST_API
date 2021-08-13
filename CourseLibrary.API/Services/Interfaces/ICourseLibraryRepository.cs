using CourseLib.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseLibrary.API.Services.Interfaces
{
    public interface ICourseLibraryRepository
    {    
        Task<IEnumerable<Course>> GetCoursesAsync(Guid authorId);
        Task<Course> GetCourseAsync(Guid authorId, Guid courseId);
        Task AddCourseAsync(Guid authorId, Course course);
        Task UpdateCourseAsync(Course course);
        Task DeleteCourseAsync(Course course);
        Task<bool> SaveAsync();
    }
}