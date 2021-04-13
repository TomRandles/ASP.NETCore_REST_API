using System;

namespace CourseLib.Domain.Utilities
{
    public static class DateCalculations
    {
        public static int CalculateAgeFromDateOfBirth(this DateTimeOffset dateTimeOffset, 
                                                      DateTimeOffset? dateOfDeath)
        {
            var dateToCalculateEndAge = DateTime.UtcNow;

            if (dateOfDeath != null)
                dateToCalculateEndAge = dateOfDeath.Value.UtcDateTime;    

            int age = dateToCalculateEndAge.Year - dateTimeOffset.Year;

            if (dateToCalculateEndAge < dateTimeOffset.AddYears(age))
            {
                age--;
            }
            return age;
        }
    }
}