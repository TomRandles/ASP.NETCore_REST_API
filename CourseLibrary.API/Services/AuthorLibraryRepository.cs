using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models.ResourceParameters;
using CourseLibrary.API.Services.Interfaces;
using CourseLibrary.Data.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Services
{
    public class AuthorLibraryRepository : IAuthorLibraryRepository, IDisposable
    {
        private readonly CourseLibraryContext _context;
        private readonly IPropertyMappingService propertyMappingService;

        public AuthorLibraryRepository(CourseLibraryContext context, 
                                       IPropertyMappingService propertyMappingService )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            this.propertyMappingService = propertyMappingService ?? 
                throw new ArgumentNullException(nameof(propertyMappingService)); 
        }
        public async Task AddAuthorAsync(Author author)
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
            await _context.Authors.AddAsync(author);
            await SaveAsync();
        }
        public async Task<bool> AuthorExistsAsync(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            return await _context.Authors.AnyAsync(a => a.Id == authorId);
        }

        public async Task DeleteAuthorAsync(Author author)
        {
            if (author == null)
            {
                throw new ArgumentNullException(nameof(author));
            }

            _context.Authors.Remove(author);
            await SaveAsync();
        }        
        public async Task<Author> GetAuthorAsync(Guid authorId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            return await _context.Authors.FirstOrDefaultAsync(a => a.Id == authorId);
        }

        public async Task<IEnumerable<Author>> GetAuthorsAsync()
        {
            return await _context.Authors.ToListAsync<Author>();
        }
        public async Task<IEnumerable<Author>> GetAuthorsAsync(IEnumerable<Guid> authorIds)
        {
            if (authorIds == null)
            {
                throw new ArgumentNullException(nameof(authorIds));
            }
            return await _context.Authors.Where(a => authorIds.Contains(a.Id))
                                 .OrderBy(a => a.FirstName)
                                 .OrderBy(a => a.LastName)
                                 .ToListAsync();
        }
        public async Task UpdateAuthorAsync(Author author)
        {
            // no code in this implementation
            await Task.CompletedTask;
        }
        public async Task<bool> SaveAsync()
        {
            var retVal = await _context.SaveChangesAsync() >= 0;
            return retVal;
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
        public async Task<PagedList<Author>> GetAuthorsAsync(AuthorsResourceParameters resourceParameters)
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

                // ApplySort - generic extension method on IQueryable available to all resources
                collection = collection.ApplySort(resourceParameters.OrderBy, authorPropertyMapping);
            }

            // Add paging after search and filtering 
            var pagedList = await PagedList<Author>.CreateAsync(collection,
                                                                resourceParameters.PageNumber,
                                                                resourceParameters.PageSize);

            return pagedList;
        }
    }
}