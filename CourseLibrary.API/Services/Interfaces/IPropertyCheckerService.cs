namespace CourseLibrary.API.Services.Interfaces
{
    public interface IPropertyCheckerService
    {
        bool TypeHasProperties<T>(string fields);
    }
}