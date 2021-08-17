using CourseLib.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLib.Domain.ValidationAttributes
{
    // Class level input validation - custom validation attribute
    // Inherits from ValidationAttribute -  takes part in the automated validation process
    // validationContext - provides access to object to validate - cast to object type CourseForManipulation
    // Can also be used for property validation

    public class CourseTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var course = (CourseForManipulation)validationContext.ObjectInstance;
            if (course.Title == course.Description)
            {
                // Error message contents will be default or ErrorMessage value from attribute instance:
                // [CourseTitleMustBeDifferentFromDescription(
                //     ErrorMessage = "The Title and Description must be different")]
                return new ValidationResult(ErrorMessage,
                    new[] { nameof(CourseForManipulation) });
            }
            return ValidationResult.Success;
        }
    }
}