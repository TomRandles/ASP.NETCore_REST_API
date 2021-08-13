using CourseLib.Domain.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models.ResourceParameters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseLibrary.API.Services.Interfaces
{
    public interface IAuthorLibraryRepository
    {    
        Task<IEnumerable<Author>> GetAuthorsAsync();
        Task<PagedList<Author>> GetAuthorsAsync(AuthorsResourceParameters resourceParameters);
        Task<Author> GetAuthorAsync(Guid authorId);
        Task<IEnumerable<Author>> GetAuthorsAsync(IEnumerable<Guid> authorIds);
        Task AddAuthorAsync(Author author);
        Task DeleteAuthorAsync(Author author);
        Task UpdateAuthorAsync(Author author);
        Task<bool> AuthorExistsAsync(Guid authorId);
        Task<bool> SaveAsync();
    }
}