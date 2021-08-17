using CourseLib.Domain.Entities;
using CourseLibrary.API.Services.Interfaces;
using CourseLibrary.Data.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Services
{
    public class CourseLibraryRepository : ICourseLibraryRepository, IDisposable
    {
        private readonly CourseLibraryContext _context;
        private readonly IPropertyMappingService propertyMappingService;

        public CourseLibraryRepository(CourseLibraryContext context, IPropertyMappingService propertyMappingService )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            this.propertyMappingService = propertyMappingService ?? 
                throw new ArgumentNullException(nameof(propertyMappingService)); 
        }

        public async Task AddCourseAsync(Guid authorId, Course course)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }
            // always set the AuthorId to the passed-in authorId
            course.AuthorId = authorId;
            await _context.Courses.AddAsync(course);
            await SaveAsync();
        }         

        public async Task DeleteCourseAsync(Course course)
        {
            _context.Courses.Remove(course);
            await SaveAsync();
        }
  
        public async Task<Course> GetCourseAsync(Guid authorId, Guid courseId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }
            if (courseId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(courseId));
            }
            return await _context.Courses
                                 .Where(c => c.AuthorId == authorId && c.Id == courseId)
                                 .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesAsync(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }
            return await _context.Courses
                                 .Where(c => c.AuthorId == authorId)
                                 .OrderBy(c => c.Title).ToListAsync();
        }

        public async Task UpdateCourseAsync(Course course)
        {
           
            // EF core - tracker marks entity as modified. No extra code required. 
            // SaveAsync will ensure modifications are saved in Db
            await SaveAsync();
        }
        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
               // dispose resources when needed
            }
        }
    }
}