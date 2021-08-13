using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CourseLibrary.API.Services.Interfaces;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CourseController : ControllerBase
    {
        private readonly IAuthorLibraryRepository _authorRepository;
        private readonly ICourseLibraryRepository _courseRepository;
        private readonly IMapper _mapper;

        public CourseController(IAuthorLibraryRepository authorRepository, 
                                ICourseLibraryRepository courseRepository, 
                                IMapper mapper)
        {
            _authorRepository = authorRepository
                    ?? throw new ArgumentNullException(nameof(authorRepository));
            _courseRepository = courseRepository
                ?? throw new ArgumentNullException(nameof(courseRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet(Name = "GetCoursesForAuthor")]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetCoursesForAuthor(Guid authorId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }
            var courses = await _courseRepository.GetCoursesAsync(authorId);
            var coursesToReturn = _mapper.Map<IEnumerable<CourseDto>>(courses);

            return Ok(coursesToReturn);
        }

        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        [HttpHead("{courseId}")]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var course = await _courseRepository.GetCourseAsync(authorId, courseId);
            if (course == null)
                NotFound();
            var courseToReturn = _mapper.Map<CourseDto>(course);
            return Ok(courseToReturn);
        }

        [HttpPost(Name ="CreateCourseForAuthor")]
        public async Task<ActionResult<CourseDto>> CreateCourseForAuthor(Guid authorId,
                                                                         [FromBody] CourseCreateDto course)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            try
            {
                var courseEntity = _mapper.Map<Course>(course);
                await _courseRepository.AddCourseAsync(authorId, courseEntity);

                var courseToReturn = _mapper.Map<CourseDto>(courseEntity);

                // Return - successful post - 201 - CreatedAtRoute() - 201 SC and a location header with
                // URI for newly created Course
                return CreatedAtRoute("GetCourseForAuthor",
                                      new { authorId = authorId, courseId = courseEntity.Id },
                                      courseToReturn);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        // Use IActionResult for return type - nothing or a created course returned
        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourseForAuthor(Guid authorId,
                                                               Guid courseId,
                                                               CourseUpdateDto course)
        {
            //Check if author exists
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            //Check if course exists
            var courseForAuthorEntity = await _courseRepository.GetCourseAsync(authorId, courseId);
            if (courseForAuthorEntity == null)
            {
                // return NotFound();
                // Upinsert pattern - need to create the course Id
                var courseToAdd = _mapper.Map<Course>(course);
                courseToAdd.Id = courseId;
                await _courseRepository.AddCourseAsync(authorId, courseToAdd);

                //201 return
                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id }, courseToReturn);
            }

            // map 
            _mapper.Map(course, courseForAuthorEntity);
            await _courseRepository.UpdateCourseAsync(courseForAuthorEntity);
            return NoContent();
        }

        // PATCH: api/programmes/Java06
        // Programme object deserialized from json
        [HttpPatch("{courseId}")]
        public async Task<IActionResult> PartialUpdateProgrammeAsync(Guid authorId, Guid courseId,
            // CourseUpdateDto cannot contain and Id
            [FromBody] JsonPatchDocument<CourseUpdateDto> coursePatchDocument)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }
            var courseFromDb = await _courseRepository.GetCourseAsync(authorId, courseId);
            if (courseFromDb == null)
            {
                // Upserting with PATCH. Includes normal validation
                var courseDto = new CourseUpdateDto();
                coursePatchDocument.ApplyTo(courseDto, ModelState);
                if (!TryValidateModel(courseDto))
                {
                    return ValidationProblem(ModelState);
                }

                var courseToAdd = _mapper.Map<Course>(courseDto);
                courseToAdd.Id = courseId;

                await _courseRepository.AddCourseAsync(authorId, courseToAdd);

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);

                return CreatedAtRoute("GetCourseForAuthor", new { authorId, courseToReturn.Id }, courseToReturn);
            }

            var courseToPatch = _mapper.Map<CourseUpdateDto>(courseFromDb);

            // Add validation
            coursePatchDocument.ApplyTo(courseToPatch, ModelState);

            // Validate that the incoming Dto class properties. Errors captured in ModelState
            if (!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(courseToPatch, courseFromDb);
            await _courseRepository.UpdateCourseAsync(courseFromDb);
            
            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public async Task<ActionResult> DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var courseFromDb = await _courseRepository.GetCourseAsync(authorId, courseId);
            if (courseFromDb == null)
            {
                return NotFound();
            }

            await _courseRepository.DeleteCourseAsync(courseFromDb);
            return NoContent();
        }
        public override ActionResult ValidationProblem (
            [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}