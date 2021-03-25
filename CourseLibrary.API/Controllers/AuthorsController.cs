using AutoMapper;
using CourseLib.Domain.Models;
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
            [FromQuery]string mainCategory, string searchQuery)
        {
            var authors = _courseLibraryRepository.GetAuthors(mainCategory, searchQuery);
            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authors);

            return Ok(authorsToReturn);
        }

        // guid - route constraint.
        [HttpGet("{authorId}")]
        [HttpHead("{authorId}")]
        public async Task<ActionResult<AuthorDto>> GetAuthorAsync(Guid authorId)
        {
            var author = _courseLibraryRepository.GetAuthor(authorId);
            if (author == null)
                return NotFound();

            var authorToReturn = _mapper.Map<AuthorDto>(author);
            return Ok(authorToReturn);
        }
    }
}