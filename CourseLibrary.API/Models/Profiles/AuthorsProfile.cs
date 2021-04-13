using AutoMapper;
using CourseLib.Domain.Entities;
using CourseLib.Domain.Models;
using CourseLib.Domain.Utilities;

namespace CourseLibraryAPI.Models.Profiles
{
    public class AuthorsProfile : Profile
    {
        public AuthorsProfile()
        {
            CreateMap<Author, AuthorDto>()
                //Include projection
                .ForMember(dest => dest.Name,
                           opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))

                .ForMember(dest => dest.Age,
                           opt => opt.MapFrom(src => src.DateOfBirth.CalculateAgeFromDateOfBirth(src.DateOfDeath)));

            CreateMap<AuthorCreateDto, Author>();

            CreateMap<AuthorCreateWithDateOfDeathDto, Author>();

            CreateMap<Author, AuthorFullDto>();
        }
    }
}