using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseLib.Domain.Models
{
    public class AuthorCreateDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        // Represents a point in time relative to Coordinated Universal Time(UTC).
        public string DateTimeOffset { get; set; }

        public string MainCategory { get; set; }

        public ICollection<CourseCreateDto> Courses { get; set; } = new List<CourseCreateDto>();
    }
}
