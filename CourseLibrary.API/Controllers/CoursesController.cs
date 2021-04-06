using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseLib.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseRepository, IMapper mapper)
        {
            this._courseRepository = courseRepository
                ?? throw new ArgumentNullException(nameof(courseRepository));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_courseRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var courses = _courseRepository.GetCourses(authorId);
            var coursesToReturn = _mapper.Map<IEnumerable<CourseDto>>(courses);

            return Ok(coursesToReturn);
        }


        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        [HttpHead("{courseId}")]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var course = _courseRepository.GetCourse(authorId, courseId);
            if (course == null)
                NotFound();
            var courseToReturn = _mapper.Map<CourseDto>(course);
            return Ok(courseToReturn);
        }

        [HttpPost]
        public async Task<ActionResult<CourseDto>> CreateCourseForAuthor(Guid authorId,
                                                                         [FromBody] CourseCreateDto course)
        {
            if (!_courseRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            try
            {
                var courseEntity = _mapper.Map<Course>(course);
                _courseRepository.AddCourse(authorId, courseEntity);
                _courseRepository.Save();

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
            if (!_courseRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            //Check if course exists
            var courseForAuthorEntity = _courseRepository.GetCourse(authorId, courseId);
            if (courseForAuthorEntity == null)
            {
                // return NotFound();
                // Upinsert pattern - need to create the course Id
                var courseToAdd = _mapper.Map<Course>(course);
                courseToAdd.Id = courseId;
                _courseRepository.AddCourse(authorId, courseToAdd);
                _courseRepository.Save();

                //201 return
                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id }, courseToReturn);
            }

            // map 
            _mapper.Map(course, courseForAuthorEntity);
            _courseRepository.UpdateCourse(courseForAuthorEntity);
            _courseRepository.Save();
            return NoContent();
        }

        // PATCH: api/programmes/Java06
        // Programme object deserialized from json
        [HttpPatch("{courseId}")]
        public async Task<IActionResult> PartialUpdateProgrammeAsync(Guid authorId, Guid courseId,
            // CourseUpdateDto cannot contain and Id
            [FromBody] JsonPatchDocument<CourseUpdateDto> coursePatchDocument)
        {
            if (!_courseRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseFromDb = _courseRepository.GetCourse(authorId, courseId);
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

                _courseRepository.AddCourse(authorId, courseToAdd);
                _courseRepository.Save();

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
            _courseRepository.UpdateCourse(courseFromDb);
            _courseRepository.Save();

            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!_courseRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseFromDb = _courseRepository.GetCourse(authorId, courseId);
            if (courseFromDb == null)
            {
                return NotFound();
            }

            _courseRepository.DeleteCourse(courseFromDb);
            _courseRepository.Save();
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