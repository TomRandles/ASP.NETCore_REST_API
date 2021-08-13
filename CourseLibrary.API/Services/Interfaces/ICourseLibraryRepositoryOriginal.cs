﻿using CourseLib.Domain.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models.ResourceParameters;
using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Services.Interfaces
{
    public interface ICourseLibraryRepositoryOriginal
    {    
        IEnumerable<Course> GetCourses(Guid authorId);
        Course GetCourse(Guid authorId, Guid courseId);
        void AddCourse(Guid authorId, Course course);
        void UpdateCourse(Course course);
        void DeleteCourse(Course course);
        IEnumerable<Author> GetAuthors();
        PagedList<Author> GetAuthors(AuthorsResourceParameters resourceParameters);
        Author GetAuthor(Guid authorId);
        IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds);
        void AddAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);
        bool AuthorExists(Guid authorId);
        bool Save();
    }
}
