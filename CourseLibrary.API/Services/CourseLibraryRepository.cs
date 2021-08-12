﻿using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models.ResourceParameters;
using CourseLibrary.API.Services.Interfaces;
using CourseLibrary.Data.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public void AddCourse(Guid authorId, Course course)
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
            _context.Courses.Add(course); 
        }         

        public void DeleteCourse(Course course)
        {
            _context.Courses.Remove(course);
        }
  
        public Course GetCourse(Guid authorId, Guid courseId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            if (courseId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            return _context.Courses
              .Where(c => c.AuthorId == authorId && c.Id == courseId).FirstOrDefault();
        }

        public IEnumerable<Course> GetCourses(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            return _context.Courses
                        .Where(c => c.AuthorId == authorId)
                        .OrderBy(c => c.Title).ToList();
        }

        public void UpdateCourse(Course course)
        {
           
            _context.Attach(course).State = EntityState.Modified;
        }

        public void AddAuthor(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            // the repository fills the id (instead of using identity columns)
            author.Id = Guid.NewGuid();

            foreach (var course in author.Courses)
            {
                course.Id = Guid.NewGuid();
            }

            _context.Authors.Add(author);
        }

        public bool AuthorExists(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            return _context.Authors.Any(a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            _context.Authors.Remove(author);
        }
        
        public Author GetAuthor(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            return _context.Authors.FirstOrDefault(a => a.Id == authorId);
        }

        public IEnumerable<Author> GetAuthors()
        {
            return _context.Authors.ToList<Author>();
        }
         
        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            if (authorIds == null)
            {
                throw new ArgumentNullException(nameof(authorIds));
            }

            return _context.Authors.Where(a => authorIds.Contains(a.Id))
                .OrderBy(a => a.FirstName)
                .OrderBy(a => a.LastName)
                .ToList();
        }

        public void UpdateAuthor(Author author)
        {
            // no code in this implementation
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
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

        public PagedList<Author> GetAuthors(AuthorsResourceParameters resourceParameters)
        {
            if (resourceParameters == null)
                throw new ArgumentNullException(nameof(resourceParameters));

            // Cast the Authors db set as IQueryable<Author>. Can use for filters (Where) or searches as
            // required. Avail of deferred execution facility.
            var collection = _context.Authors as IQueryable<Author>;

            if (!string.IsNullOrWhiteSpace(resourceParameters.MainCategory))
            {
                var mainCategory = resourceParameters.MainCategory.Trim();
                collection = _context.Authors.Where(a => a.MainCategory == mainCategory);
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.SearchQuery))
            {
                var searchQuery = resourceParameters.SearchQuery.Trim();
                collection = collection.Where(a => a.MainCategory.Contains(searchQuery)
                             || a.FirstName.Contains(searchQuery)
                             || a.LastName.Contains(searchQuery));
            }

            // Implement default ordering
            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                
                // Get property mapping dictionary
                var authorPropertyMapping = propertyMappingService.GetPropertyMapping<AuthorDto, Author>();

                collection = collection.ApplySort(resourceParameters.OrderBy, authorPropertyMapping);
            }

            // Add paging after search and filtering 
            var pagedList = PagedList<Author>.Create(collection,
                                                     resourceParameters.PageNumber,
                                                     resourceParameters.PageSize);

            return pagedList;
        }
    }
}