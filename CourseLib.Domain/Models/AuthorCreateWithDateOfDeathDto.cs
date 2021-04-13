using System;

namespace CourseLib.Domain.Models
{
    public class AuthorCreateWithDateOfDeathDto : AuthorCreateDto
    {
        public DateTimeOffset? DateOfDeath { get; set; }
    }
}