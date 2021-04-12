using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, 
                                 IMapper mapper, 
                                 IPropertyMappingService propertyMappingService,
                                 IPropertyCheckerService propertyCheckerService)
        {
            this._courseLibraryRepository = courseLibraryRepository ??
                 throw new ArgumentNullException(nameof(courseLibraryRepository));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this._propertyMappingService = propertyMappingService ?? 
                throw new ArgumentNullException(nameof(propertyMappingService));
            this._propertyCheckerService = propertyCheckerService ?? 
                throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name ="GetAuthors")]
        [HttpHead]
        public async Task<IActionResult> GetAuthorsAsync(
            // Complex type AuthorsResourceParameters requires a [FromQuery] attribute - else will result in 415 SC
            [FromQuery] AuthorsResourceParameters resourceParameters)
        {

            // Validate incoming orderBy, if present
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            // Check fields. if present, are valid properties
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            var authors = _courseLibraryRepository.GetAuthors(resourceParameters);

            var previousPageLink = authors.HasPrevious ?
                CreateAuthorsResourceUri(resourceParameters, ResourceUriType.PreviousPage) : null;

            var nextPageLink = authors.HasNext ?
                CreateAuthorsResourceUri(resourceParameters, ResourceUriType.NextPage) : null;

            var paginationMetaData = new
            {
                totalCount = authors.TotalCount,
                pageSize = authors.PageSize,
                currentPage = authors.CurrentPage,
                totalPages = authors.TotalPages,
                previousPageLink,
                nextPageLink
            };

            //Add to header
            Response.Headers.Add("X-Pagination", 
                JsonSerializer.Serialize(paginationMetaData));

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authors)
                                         .ShapeData(resourceParameters.Fields);

            return Ok(authorsToReturn);
        }

        [HttpGet("{authorId}", Name="GetAuthor")]
        [HttpHead("{authorId}")]
        public async Task<ActionResult<AuthorDto>> GetAuthorAsync(Guid authorId, string fields)
        {
            // Check fields, if present, are valid properties
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var author = _courseLibraryRepository.GetAuthor(authorId);

            if (author == null)
                return NotFound();

            var authorToReturn = _mapper.Map<AuthorDto>(author).ShapeData(fields);
            return Ok(authorToReturn);
        }
        [HttpPost]
        public async Task<ActionResult<AuthorDto>> CreateAuthor([FromBody] AuthorCreateDto author)
        {
            try
            {
                var authorEntity = _mapper.Map<Author>(author);
                _courseLibraryRepository.AddAuthor(authorEntity);
                _courseLibraryRepository.Save();

                var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

                // Return - successful post - 201 - CreatedAtRoute() - 201 SC and a location header with
                // URI for newly created Author
                return CreatedAtRoute("GetAuthor", new { authorId = authorEntity.Id}, authorToReturn);

            } catch (Exception )
            {
                // temp - need to change
                return BadRequest();
            }
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var author = _courseLibraryRepository.GetAuthor(authorId);
            if (author == null)
            {
                return NotFound();
            }

            // NB - Cascade-on-Delete is on by default. So courses
            // (child objects) are deleted by default
            _courseLibraryRepository.DeleteAuthor(author);
            _courseLibraryRepository.Save();
            
            return NoContent();
        }

        private string CreateAuthorsResourceUri (AuthorsResourceParameters resourceParameters, ResourceUriType type)
        {
            switch(type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                        // Query string parameters
                        new
                        {
                            fields = resourceParameters.Fields,
                            orderBy = resourceParameters.OrderBy,
                            pageNumber=resourceParameters.PageNumber -1,
                            pageSize=resourceParameters.PageSize,
                            mainCategory = resourceParameters.MainCategory,
                            searchQuery = resourceParameters.SearchQuery
                        });
                case ResourceUriType.NextPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = resourceParameters.Fields,
                            orderBy = resourceParameters.OrderBy,
                            pageNumber = resourceParameters.PageNumber + 1,
                            pageSize = resourceParameters.PageSize,
                            mainCategory = resourceParameters.MainCategory,
                            searchQuery = resourceParameters.SearchQuery
                        });
                default:
                    // Return current page as default
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = resourceParameters.Fields,
                            orderBy = resourceParameters.OrderBy,
                            pageNumber = resourceParameters.PageNumber,
                            pageSize = resourceParameters.PageSize,
                            mainCategory = resourceParameters.MainCategory,
                            searchQuery = resourceParameters.SearchQuery
                        });
            }
        }
    }
}