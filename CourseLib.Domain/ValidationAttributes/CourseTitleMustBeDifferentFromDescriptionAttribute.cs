using CourseLib.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLib.Domain.ValidationAttributes
{
    public class CourseTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var course = (CourseForManipulation)validationContext.ObjectInstance;
            if (course.Title == course.Description)
            {
                return new ValidationResult(ErrorMessage,
                    new[] { nameof(CourseForManipulation) });
            }

            return ValidationResult.Success;
        }
    }
}
