using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.Models.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            this._courseLibraryRepository = courseLibraryRepository
                ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorsAsync(
            // Complex type AuthorsResourceParameters requires a [FromQuery] attribute - else will result in 415 SC
            [FromQuery] AuthorsResourceParameters resourceParameters)
        {
            var authors = _courseLibraryRepository.GetAuthors(resourceParameters.MainCategory,
                                                              resourceParameters.SearchQuery);

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authors);

            return Ok(authorsToReturn);
        }

        [HttpGet("{authorId}", Name="GetAuthor")]
        [HttpHead("{authorId}")]
        public async Task<ActionResult<AuthorDto>> GetAuthorAsync(Guid authorId)
        {
            var author = _courseLibraryRepository.GetAuthor(authorId);
            if (author == null)
                return NotFound();

            var authorToReturn = _mapper.Map<AuthorDto>(author);
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
    }
}