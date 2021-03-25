using AutoMapper;
using CourseLib.Domain.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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


        [HttpGet("{courseId}")]
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
    }
}