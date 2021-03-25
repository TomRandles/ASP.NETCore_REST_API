using System;

namespace CourseLib.Domain.Utilities
{
    public static class DateCalculations
    {
        public static int CalculateAgeFromDateOfBirth(this DateTimeOffset dateTimeOffset)
        {
            var currentDate = DateTime.UtcNow;
            int age = currentDate.Year - dateTimeOffset.Year;

            if (currentDate < dateTimeOffset.AddYears(age))
            {
                age--;
            }
            return age;
        }
    }
}