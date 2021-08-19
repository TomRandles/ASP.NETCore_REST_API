using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers
{
    // Use a different class for a single object for performance reasons
    public static class ObjectExtensions
    {
        public static ExpandoObject ShapeData<TSource>(this TSource source, string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var objectToReturn = new ExpandoObject();

            // Use reflection to get the property info. This will be done on one
            // object, as reflection is expensive.

            // Check for empty fields
            if (string.IsNullOrWhiteSpace(fields))
            {
                // Return all propertys - no filter received, public and instance
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    
                
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = propertyInfo.GetValue(source);

                    // Add the field to the expando object
                    ((IDictionary<string, object>)objectToReturn).Add(propertyInfo.Name,
                                                                        propertyValue);
                }
                return objectToReturn;
            }
            
            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                // trim field of leading or trailing spaces.
                var propertyName = field.Trim();

                // Use reflection to get the property on the source object
                // Focus on public and instance fields. 
                // Specifying a binding flag overwrites existing binding flags.
                var propertyInfo = typeof(TSource).GetProperty(propertyName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"Property {propertyName} was not found on" +
                        $" {typeof(TSource)}");
                }
                var propertyValue = propertyInfo.GetValue(source);

                // Add the field to the expando object
                ((IDictionary<string, object>)objectToReturn).Add(propertyInfo.Name,
                                                                 propertyValue);
            }    
            
            // Return object
            return objectToReturn;
        }
    }
}