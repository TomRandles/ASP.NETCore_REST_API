using CourseLibrary.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CourseLibrary.API.Controllers
{
    // Supports self discovery with a root document
    // Root document - starting point for consumers of the API.
    // API consumers can start at this point to learn how to interact with the rest of the API
    // https://host/api 
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot()
        {
            // Generate links to the document itself and links to actions at root level

            var links = new List<LinkDto>();

            links.Add(
                new LinkDto(Url.Link("GetRoot", new { }),
                "self",
                "GET"));

            links.Add(
                new LinkDto(Url.Link("GetAuthors", new { }),
               "authors",
               "GET"));

            links.Add(
                new LinkDto(Url.Link("CreateAuthor", new { }),
                "create_author",
                "POST"));

            return Ok(links);
        }
    }
}
