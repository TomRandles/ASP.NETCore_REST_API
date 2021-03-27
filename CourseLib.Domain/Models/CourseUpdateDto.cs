using CourseLib.Domain.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLib.Domain.Models
{

    public class CourseUpdateDto : CourseForManipulation
    {

        [Required(ErrorMessage ="Description is required.")]
        [MaxLength(1500, ErrorMessage = "Maximum length is 1500 characters.")]
        public override string Description { get; set; }

    }
}
