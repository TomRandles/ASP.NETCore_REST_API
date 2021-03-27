using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionsController(ICourseLibraryRepository courseLibraryRepository, 
                                           IMapper mapper)
        {
            this._courseLibraryRepository = courseLibraryRepository
                ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection(
        [FromRoute]
        // Need to provide custom model binders
        [ModelBinder(BinderType =typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }
            var authorEntities = _courseLibraryRepository.GetAuthors(ids);

            // NB - check that all authors were found
            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authorsToReturn);
        }

        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(
            IEnumerable<AuthorCreateDto> authors)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authors);
            foreach (var author in authorEntities)
            {
                _courseLibraryRepository.AddAuthor(author);
            }
            _courseLibraryRepository.Save();

            // Need to return a list of author resources
            var authorsCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            string idsString = string.Join(",", authorsCollectionToReturn.Select(i => i.Id));
            return CreatedAtRoute("GetAuthorCollection", new { ids = idsString }, authorsCollectionToReturn);
        }
    }
}