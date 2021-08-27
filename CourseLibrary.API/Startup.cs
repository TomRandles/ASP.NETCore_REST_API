using CourseLibrary.API.Services;
using CourseLibrary.API.Services.Interfaces;
using CourseLibrary.Data.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Linq;

namespace CourseLibrary.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Global app expiration rules
            services.AddHttpCacheHeaders((expirationModelOptions ) =>
            {
                expirationModelOptions.MaxAge = 60;
                expirationModelOptions.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private;
            },
            // Global app cache validation rules
            (validationModelOptions) =>
            {
                validationModelOptions.MustRevalidate = true;
            });

            // Adding a cache store with the ResponseCaching middleware
            services.AddResponseCaching();

            services.AddControllers(setupActions =>
            {
                // Will return a 406 - media not supported if Accept header media type not supported.
                setupActions.ReturnHttpNotAcceptable = true;
                // CacheProfiles - a dictionary - add key and new CacheProfile object
                setupActions.CacheProfiles.Add("240SecondsCacheProfile",
                                                   new CacheProfile()
                                                   {
                                                       Duration = 240
                                                   });
            })
              // Order important here between json (now default again) and xml. Accept & Content-type header
              // overrides these defaults. 
              // Preferred Json package as of 3.x 
              .AddNewtonsoftJson(setupAction =>
              {
                  setupAction.SerializerSettings.ContractResolver =
                      new CamelCasePropertyNamesContractResolver();
              })
              // .Net 3.x preferred way of adding formatters
              // Add XML formatter. This one supports date time offset value, used in code
              .AddXmlDataContractSerializerFormatters()
              .ConfigureApiBehaviorOptions(setupAction =>
                {
                    // Customising validation error responses
                    // Executed when the model state is invalid
                    setupAction.InvalidModelStateResponseFactory = context =>
                    {
                        // create a problem details object
                        var problemDetailsFactory = context.HttpContext.RequestServices
                                                           .GetRequiredService<ProblemDetailsFactory>();
                        // translate errors to RFC 7807 format
                        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                                                                   context.HttpContext,
                                                                   context.ModelState);

                        // add additional info not added by default
                        problemDetails.Detail = "See the errors field for details.";
                        problemDetails.Instance = context.HttpContext.Request.Path;

                        // find out which status code to use
                        var actionExecutingContext =
                            context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                        // if there are ModelState errors & all keys were correctly
                        // found/parsed we're dealing with validation errors
                        if ((context.ModelState.ErrorCount > 0) &&
                            (actionExecutingContext?.ActionArguments.Count == context.ActionDescriptor.Parameters.Count))
                        {
                            problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                            problemDetails.Title = "One or more validation errors occurred.";

                            return new UnprocessableEntityObjectResult(problemDetails)
                            {
                                ContentTypes = { "application/problem+json" }
                            };
                        }

                        // if one of the keys wasn't correctly found / couldn't be parsed
                        // we're dealing with null/unparsable input
                        problemDetails.Status = StatusCodes.Status400BadRequest;
                        problemDetails.Title = "One or more errors on input occurred.";
                        return new BadRequestObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });

            // AddTransient – advised by ASP.NET team for lightweight stateless services.
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Scoped - equal to or shorter than the Db Context (scoped)
            // transient - New instance for every call. Would lose any state our repository might hold
            // if requested by multiple parts of the code.
            // Scoped - correct option
            services.AddScoped<IAuthorLibraryRepository, AuthorLibraryRepository>();
            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            // DbContext - scoped lifetime by default. Dispose of the db context after every request.
            services.AddDbContext<CourseLibraryContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DbConnection"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // Configuration for vendor specific media types
            // Add media type to existing formatter
            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters
                    .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

                if (newtonsoftJsonOutputFormatter != null)
                {
                    // Adds support for this media type application wide
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
                }
            });

            // Transient - for lightweight, stateless services - recommended
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            // Property checking service for incoming HTTP traffic
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

            // Automapper for Dto to/from entity object mapping
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CourseLibrary.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CourseLibrary.API v1"));
            }
            else
            {
                // Handler used in production which is configurable
                // Will generate 500 sc - server errors. 
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        Log.Fatal("A critical error has occured.");
                        await context.Response.WriteAsync("An unexpected server fault occurred");
                    });
                });
            }

            // Make sure UseResponseCaching() and UseHttpCacheHeaders() are added before UseRouting() and UseEndpoints()
            // Ensures that the cache middleware can serve something up before the rest of the MVC logic is routed to or executed. 

            app.UseResponseCaching();

            // Ensures that the request pipeline does not continue to API routing and endpoints if cache validation ok
            // NB: add after UseResponseCaching() - ETag generating middleware not requred if response can be served from the
            // cache
            app.UseHttpCacheHeaders();

            app.UseStatusCodePages();

            app.UseHttpsRedirection();

            // Marks point where routing decisions are made, i.e. where an endpoint is selected.
            app.UseRouting();

            app.UseAuthorization();

            // Marks the point where the selected endpoint is executed.
            // Can now inject middleware between the selection and execution of endpoint.
            // E.g. Authentication middleware.
            app.UseEndpoints(endpoints =>
            {
                // Adds endpoints for controller actions to routing. NB: no routes are specified.
                // Will use attribute routing - best practice. Preferred for APIs.
                endpoints.MapControllers();
            });
        }
    }
}