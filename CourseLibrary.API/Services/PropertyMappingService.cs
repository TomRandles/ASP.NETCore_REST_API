using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
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

        // Use marker interface to resolve 
        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();
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
