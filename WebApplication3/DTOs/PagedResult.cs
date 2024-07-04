namespace WebApplication3.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalPages { get; set; }
    }
}
