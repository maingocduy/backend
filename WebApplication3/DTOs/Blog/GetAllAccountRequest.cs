namespace WebApplication3.DTOs.Blog
{
    public class GetAllBlogRequest
    {
        public int pageNumber { get; set; } =1;
        public string? keyword { get; set; } = null;

        public bool? approved { get; set; } = null;
        public int pageSize { get; set; } = 6;
    }
}
