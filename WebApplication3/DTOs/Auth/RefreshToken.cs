namespace WebApplication3.DTOs.Auth
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }
    }
}
