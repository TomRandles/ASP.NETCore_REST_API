using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IAuthorLibraryRepository _authorLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ICourseLibraryRepository courseLibraryRepository, 
                                           IAuthorLibraryRepository authorLibraryRepository,
            
                                           IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository
                ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
            _authorLibraryRepository = authorLibraryRepository
                ?? throw new ArgumentNullException(nameof(authorLibraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        //Pass in a comma separated list of guids
        // Round brackets with array of guidskk
        
        [HttpGet("({ids})", Name = "GetAuthorCollectionAsync")]
        public async Task<IActionResult> GetAuthorCollectionAsync(
        // [FromRoute] - Ensure the framework gets the guids from the route
        [FromRoute]
        // Need to provide custom model binders - implent IModelBinder
        [ModelBinder(BinderType =typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }
            var authorEntities = await _authorLibraryRepository.GetAuthorsAsync(ids);

            // NB - check that all authors were found
            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authorsToReturn);
        }

        [HttpPost]
        public async  Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollectionAsync(
            IEnumerable<AuthorCreateDto> authors)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authors);
            foreach (var author in authorEntities)
            {
                await _authorLibraryRepository.AddAuthorAsync(author);
            }

            // Need to return a list of author resources
            var authorsCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            string idsString = string.Join(",", authorsCollectionToReturn.Select(i => i.Id));
            return CreatedAtRoute("GetAuthorCollectionAsync", new { ids = idsString }, authorsCollectionToReturn);
        }
    }
}