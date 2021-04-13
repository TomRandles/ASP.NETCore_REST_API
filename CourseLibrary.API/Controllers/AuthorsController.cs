﻿using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.ActionConstraints;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Models.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet(Name = "GetAuthors")]
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

            var paginationMetaData = new
            {
                totalCount = authors.TotalCount,
                pageSize = authors.PageSize,
                currentPage = authors.CurrentPage,
                totalPages = authors.TotalPages,
            };

            //Add to header
            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetaData));

            var links = CreateLinksForAuthors(resourceParameters, authors.HasNext, authors.HasPrevious);

            var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authors)
                                         .ShapeData(resourceParameters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"]);
                authorAsDictionary.Add("links", authorLinks);
                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };
            return Ok(linkedCollectionResource);
        }

        // Pass all types acceptable for this action. NB - restrictive. Returns 406 if not found.
        // Can be added to controller or globally
        [Produces("application/json", 
                  "application/vnd.marvin.hateoas+json",
                  "application/vnd.marvin.author.full+json",
                  "application/vnd.marvin.author.full.hateoas+json",
                  "application/vnd.marvin.author.friendly+json",
                  "application/vnd.marvin.author.friendly.hateoas+json"
                 )]
        [HttpGet("{authorId}", Name = "GetAuthor")]
        [HttpHead("{authorId}")]
        public async Task<ActionResult<AuthorDto>> GetAuthorAsync(Guid authorId,
                                                                  string fields,
                                      // FromHeader - bind the property to a header entry
                                      [FromHeader(Name = "Accept")] string mediaType)
        {
            // Check mediaType - assumes only one type passed
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            // Check fields, if present, are valid properties
            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            var author = _courseLibraryRepository.GetAuthor(authorId);

            if (author == null)
                return NotFound();

            IEnumerable<LinkDto> links = new List<LinkDto>();

            var includeLinks = parsedMediaType.SubTypeWithoutSuffix
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            if (includeLinks)
            {
                links = CreateLinksForAuthor(authorId, fields);
            }

            // subtype without suffix - or - subtype without suffix without hateoas
            var primaryMediaType = includeLinks ? parsedMediaType.SubTypeWithoutSuffix
                .Substring(0, parsedMediaType.SubTypeWithoutSuffix.Length - 8)
                : parsedMediaType.SubTypeWithoutSuffix;

            if (primaryMediaType == "vnd.marvin.author.full")
            {
                // Cast return same as Expando object
                var fullResourceToReturn = _mapper.Map<AuthorFullDto>(author).ShapeData(fields)
                                                    as IDictionary<string, object>;
                if (includeLinks)
                {
                    fullResourceToReturn.Add("links",links);
                }

                return Ok(fullResourceToReturn);
            }

            // Return friendly author
            var friendlyResourceToReturn = _mapper.Map<AuthorDto>(author).ShapeData(fields)
                                                    as IDictionary<string, object>;
            if (includeLinks)
            {
                friendlyResourceToReturn.Add("links", links);
            }

            return Ok(friendlyResourceToReturn);
        }

        [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
        [RequestHeaderMatchesMediaType("Content-Type",
                                       "application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        [Consumes("application/vnd.marvin.authorforcreationwithdateofdeath+json")]
        public async Task<ActionResult<AuthorDto>> CreateAuthorWithDateOfDeath([FromBody] AuthorCreateWithDateOfDeathDto author)
        {
            try
            {
                var authorEntity = _mapper.Map<Author>(author);
                _courseLibraryRepository.AddAuthor(authorEntity);
                _courseLibraryRepository.Save();

                //Implement HATEOAS
                var links = CreateLinksForAuthor(authorEntity.Id);

                // Cast as expando object - IDictionary<string, object>
                var linkedResourceToReturn = _mapper.Map<AuthorDto>(authorEntity)
                                                    .ShapeData(null)
                                             as IDictionary<string, object>;

                // add links property 
                linkedResourceToReturn.Add("links", links);

                //HTTP SC 201 response
                return CreatedAtRoute("GetAuthor",
                                      new { authorId = linkedResourceToReturn["Id"] },
                                      linkedResourceToReturn);


            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        [HttpPost(Name = "CreateAuthor")]
        [RequestHeaderMatchesMediaType("Content-Type", 
                                       "application/json", 
                                       "application/vnd.marvin.authorforcreation+json")]
        [Consumes("application/json", "application/vnd.marvin.authorforcreation+json")]
        public async Task<ActionResult<AuthorDto>> CreateAuthor([FromBody] AuthorCreateDto author)
        {
            try
            {
                var authorEntity = _mapper.Map<Author>(author);
                _courseLibraryRepository.AddAuthor(authorEntity);
                _courseLibraryRepository.Save();

                //Implement HATEOAS
                var links = CreateLinksForAuthor(authorEntity.Id);

                // Cast as expando object - IDictionary<string, object>
                var linkedResourceToReturn = _mapper.Map<AuthorDto>(authorEntity)
                                                    .ShapeData(null)
                                             as IDictionary<string, object>;

                // add links property 
                linkedResourceToReturn.Add("links", links);

                //HTTP SC 201 response
                return CreatedAtRoute("GetAuthor",
                                      new { authorId = linkedResourceToReturn["Id"] },
                                      linkedResourceToReturn);


            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }

        [HttpDelete("{authorId}", Name = "DeleteAuthor")]
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

        private string CreateAuthorsResourceUri(AuthorsResourceParameters resourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                        // Query string parameters
                        new
                        {
                            fields = resourceParameters.Fields,
                            orderBy = resourceParameters.OrderBy,
                            pageNumber = resourceParameters.PageNumber - 1,
                            pageSize = resourceParameters.PageSize,
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

                case ResourceUriType.Current:
                // Return current page
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

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters resourceParameters,
                                                           bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            //
            links.Add(
                new LinkDto(CreateAuthorsResourceUri(
                    resourceParameters, ResourceUriType.Current),
                "self", "GET")
                );
            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateAuthorsResourceUri(
                        resourceParameters, ResourceUriType.PreviousPage),
                    "previousPage", "GET")
                    );
            }

            if (hasNext)
            {
                links.Add(
                    new LinkDto(CreateAuthorsResourceUri(
                        resourceParameters, ResourceUriType.NextPage),
                    "nextPage", "GET")
                    );
            }

            return links;
        }
        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields = null)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(new LinkDto(Url.Link("GetAuthor", new { authorId }),
                    "self",
                    "GET")
                );
            }
            else
            {
                links.Add(new LinkDto(Url.Link("GetAuthor", new { authorId, fields }),
                    "self",
                    "GET")
                );
            }
            // Add delete link
            links.Add(new LinkDto(Url.Link("DeleteAuthor", new { authorId }),
                      "delete_author",
                      "DELETE"));

            // Add create course for Author
            links.Add(new LinkDto(Url.Link("CreateCourseForAuthor", new { authorId }),
                      "create_course_for_author",
                      "POST"));

            // Add get courses for Author
            links.Add(new LinkDto(Url.Link("GetCoursesForAuthor", new { authorId }),
                      "get_courses_for_author",
                      "GET"));
            return links;
        }
    }
}