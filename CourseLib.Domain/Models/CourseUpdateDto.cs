using System.ComponentModel.DataAnnotations;

namespace CourseLib.Domain.Models
{

    // No Id included - redundant. Already contained in URI.
    // Guidance - do not put details already in the URI in the HTTP method request body
    public class CourseUpdateDto : CourseForManipulation
    {

        // Attributes from base abstract class Description property also apply - MaxLength
        [Required(ErrorMessage ="Description is required.")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}