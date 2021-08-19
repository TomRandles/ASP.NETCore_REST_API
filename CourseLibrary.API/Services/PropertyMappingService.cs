using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLibrary.API.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        // Use marker interface (interface without methods) to resolve
        // private IList<PropertyMapping<TSource, TDestination>> __propertyMapping;
        // TSource and TDestination cannot be resolved
        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();
        public Dictionary<string, PropertyMappingValue> _authorPropertyMapping { get; set; } =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id" }) },
                { "MainCategory", new PropertyMappingValue(new List<string>() { "MainCategory" }) },
                { "Age", new PropertyMappingValue(new List<string>() { "DateOfBirth" }, true ) },
                { "Name", new PropertyMappingValue(new List<string>() { "FirstName", "LastName" }) }
            };

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            // All ok if no fields present
            if (string.IsNullOrWhiteSpace(fields))
                return true;

            // split fields on coma
            var fieldSplit = fields.Split(',');

            // Iterate through properties and validate
            foreach (var field in fieldSplit)
            {
                var trimmedField = field.Trim();

                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedField :
                    trimmedField.Remove(indexOfFirstSpace);

                // find matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }


        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            // Look for matching mapping
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance " +
                $"for <{typeof(TSource)}, {typeof(TDestination)}");
        }
    }
}
