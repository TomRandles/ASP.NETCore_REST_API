using CourseLib.Domain.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLib.Domain.Models
{
    // Abstract class to remove code duplication

    // Use of class level input validation with a custom validation attribute
    // Takes over from IValidatableObject - no longer needed, so commented out
    // Custom attributes are executed before the Validate method gets called. Useful for property level 
    // validation. 
    [CourseTitleMustBeDifferentFromDescription(
        ErrorMessage = "The Title and Description must be different")]
    public abstract class CourseForManipulation // : IValidatableObject
    {

        // Will automatically return a 400 BadRequest sc validation error to API consumer 
        // part of ApiController attribute enablement on controller. 
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Maximum length is 100 characters.")]
        public string Title { get; set; }

        // Virtual - property can be overridden in derived classes 
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
