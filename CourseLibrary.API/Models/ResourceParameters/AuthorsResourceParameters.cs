namespace CourseLibrary.API.Models.ResourceParameters
{
    public class AuthorsResourceParameters
    {
        public string MainCategory { get; set; }
        public string SearchQuery { get; set; }

        // Give the pagination parameters default values
        // Max page size per resource collection rather
        // than a general max page size
        const int MAX_PAGE_SIZE = 20;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 5;
        public int PageSize
        {
            get => _pageSize;
            
            set => _pageSize = (value > MAX_PAGE_SIZE) ? MAX_PAGE_SIZE : value;
        }
        public string OrderBy { get; set; } = "Name";

        public string Fields { get; set; }
    }
}