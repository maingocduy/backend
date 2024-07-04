using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs.Account
{
    public class EnterPassRequest
    {
        public string Password { get; set; }
        

        public string Email { get; set; }

        public string Otp {  get; set; }
    }
}
