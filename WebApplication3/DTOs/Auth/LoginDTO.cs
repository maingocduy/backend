using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs.Auth
{
    public class LoginDTO
    {

        public string? Username { get; set; }
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
