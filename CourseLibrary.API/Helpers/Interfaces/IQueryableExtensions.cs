using CourseLibrary.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace CourseLibrary.API.Helpers
{

    public static class IQueryableExtensions
    {
        // Reusable generic extension method on IQueryable available to all resources
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
                                                 Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            
            if (mappingDictionary == null)
                throw new ArgumentNullException(nameof(mappingDictionary));

            // Nothing to order by if empty
            if (string.IsNullOrWhiteSpace(orderBy))
                return source;

            var orderByString = string.Empty;

            var orderBys = orderBy.Split(',');

            foreach (var orderByClause in orderBys)
            {
                // Get rid of leading/trailing spaces
                var trimmedOrderByClause = orderByClause.Trim();

                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                // remove asc or desc from orderByClause
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedOrderByClause :
                    trimmedOrderByClause.Remove(indexOfFirstSpace);

                // Search for property name in dictionary
                if (!mappingDictionary.ContainsKey(propertyName))
                    throw new ArgumentException($"Key mapping for {propertyName} not found.");

                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                // Go through the property names. Ensure orderby clauses are applied in the
                // correct order
                foreach (var destProperty in propertyMappingValue.DestinationProperties)
                {
                    // revert sort if necessary
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }

                    orderByString = orderByString + (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                        + destProperty + (orderDescending ? " descending" : " ascending");
                    
                }
            }
            return source.OrderBy(orderByString);
        }
    }
}
