using CourseLib.Domain.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLib.Domain.Models
{
    [CourseTitleMustBeDifferentFromDescription(
        ErrorMessage = "The Title and Description must be different")]
    public abstract class CourseForManipulation // : IValidatableObject
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Maximum length is 100 characters.")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "Maximum length is 1500 characters.")]
        public virtual string Description { get; set; }

        // Called automatically by server side validation
        // ValidationContext - provides access to the object. Annotation validations must pass first.
        // public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        // {
        //     var course = (CourseCreateDto)validationContext.ObjectInstance;

        //     if (Title == Description)
        //     {
        //         yield return new ValidationResult("The Title and Description should be different",
        //             new[] { "CourseCreateDto" });
        //     }
        // }

    }
}
