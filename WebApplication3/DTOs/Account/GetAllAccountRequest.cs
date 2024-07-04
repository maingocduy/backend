namespace WebApplication3.DTOs.Account
{
    public class GetAllAccountRequest
    {
        public int pageNumber { get; set; } =1;
        public string? keyword { get; set; } = null;
    }
}
