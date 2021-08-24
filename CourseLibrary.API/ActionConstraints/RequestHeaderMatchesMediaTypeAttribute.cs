using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;

namespace CourseLibrary.API.ActionConstraints
{
    // Action Constraint - Allow the selection of an action based on, for example, the content-type header.
    //                   - ensures the correct action is selected on the controller
    // Action constraint attribute
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly MediaTypeCollection _mediaTypes = new MediaTypeCollection();

        private readonly string _requestHeaderToMatch;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch, 
                                                      string mediaType, 
                                                      params string[] otherMediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch ??
                throw new ArgumentNullException(nameof(requestHeaderToMatch));

            // Check if media types are valid media types and add them to the collection
            if (MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                _mediaTypes.Add(parsedMediaType);
            }
            else
            {
                throw new ArgumentException(nameof(mediaType));
            }

            foreach( var otherMediaType in otherMediaTypes)
            {
                if (MediaTypeHeaderValue.
                    TryParse(otherMediaType, out MediaTypeHeaderValue parsedOtherMediaType))
                {
                    _mediaTypes.Add(parsedOtherMediaType);
                }
                else
                {
                    throw new ArgumentException(nameof(parsedOtherMediaType));
                }
            }
        }

        // Decides which stage the action constraint is part of - stage 0
        int IActionConstraint.Order => 0;

        // Is action valid. 
        bool IActionConstraint.Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            var parsedRequestMediaType = new MediaType(requestHeaders[_requestHeaderToMatch]);
            
            // If one of the media types match, return true
            foreach (var mediaType in _mediaTypes)
            {
                var parsedMediaType = new MediaType(mediaType);
                if (parsedRequestMediaType.Equals(parsedMediaType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}