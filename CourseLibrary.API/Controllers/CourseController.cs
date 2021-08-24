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
    // Route template - attribute route on a controller
    // Courses are children of specific author 
    [Route("api/authors/{authorId}/courses")]
    // Apply response cache profile at controller level
    [ResponseCache(CacheProfileName ="240SecondsCacheProfile")]
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
        // Sets the parameters necessary for setting appropriate headers in response caching. Cache for 120 seconds
        // Overrides controller level ResponseCache attribute settings
        [ResponseCache(Duration =120)]
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

        //
        // Use IActionResult for return type - nothing or a created course returned
        // CourseUpdateDto - contains payload for update request. 
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
                // Upsert pattern - need to create the course
                var courseToAdd = _mapper.Map<Course>(course);
                // Get course Id from URI
                courseToAdd.Id = courseId;
                await _courseRepository.AddCourseAsync(authorId, courseToAdd);

                //201 return - correct sc since a new resource has been created. 
                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id }, courseToReturn);
            }

            // EF core - tracker marks entity as modified.
            _mapper.Map(course, courseForAuthorEntity);
            await _courseRepository.UpdateCourseAsync(courseForAuthorEntity);
            // Status code 204 - no content returned
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

                return CreatedAtRoute("GetCourseForAuthor", 
                                      new { authorId, courseToReturn.Id }, 
                                      courseToReturn);
            }

            var courseToPatch = _mapper.Map<CourseUpdateDto>(courseFromDb);

            // Validation - adding ModelState argument in ApplyTo will cause any errors in the patch document to make
            // the ModelState invalid. TryValidateModel below looks after reporting validation issues found here
            coursePatchDocument.ApplyTo(courseToPatch, ModelState);

            // Add validation after completing work with jsonPatch document 
            // Validate that the incoming Dto class properties. Errors captured in ModelState
            if (!TryValidateModel(courseToPatch))
            {
                // Creates an ActionResult - sc 400 bad request with errors from modelStateDictionary
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
            
            // No response body - NoContent
            return NoContent();
        }

        // Ensure InvalidModelStateResponseFactory configured in Startup.cs is used instead of default controller
        // ValidationProblem
        // Ensures a 422 Unprocessable Entity, as configured, is returned for an invalid json patch object
        public override ActionResult ValidationProblem (
            [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}