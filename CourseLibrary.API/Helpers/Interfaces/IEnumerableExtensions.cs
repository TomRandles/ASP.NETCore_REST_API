using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers.Interfaces
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> source,
            string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var expandoObjects = new List<ExpandoObject>();

            // Use reflection to get the property info. This will be done on one
            // object, as reflection is expensive.
            var propertyInfoList = new List<PropertyInfo>();

            // Check for empty fields
            if (string.IsNullOrWhiteSpace(fields))
            {
                // Return all propertys - no filter received, public and instance
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);
                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                var fieldsAfterSplit = fields.Split(',');

                foreach (var field in fieldsAfterSplit)
                {
                    var propertyName = field.Trim();

                    var propertyInfo = typeof(TSource).GetProperty(propertyName,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} was not found on" +
                            $" {typeof(TSource)}");
                    }

                    // Add property info to list 
                    propertyInfoList.Add(propertyInfo);
                }
            }
            foreach (TSource sourceObj in source)
            {
                // Create instance of shaped object to return
                var dataShapedObject = new ExpandoObject();

                foreach (var propertyInfo in propertyInfoList)
                {
                    var propertyValue = propertyInfo.GetValue(sourceObj);

                    // Add the field to the expando object
                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name,
                                                                        propertyValue);
                }
                // Add new expando object to the list to return
                expandoObjects.Add(dataShapedObject);
            }

            // Return list
            return expandoObjects;
        }
    }
}