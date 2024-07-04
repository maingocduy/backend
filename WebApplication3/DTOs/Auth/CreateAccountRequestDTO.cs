using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs.Auth
{
    public class CreateAccountRequestDTO
    {
        [Required(ErrorMessage = "Username Required")]
        public string username { get; set; }

        [Required(ErrorMessage = "Password Required")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Required]
        [Compare("password", ErrorMessage = "Password not match.")]
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone number is required")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [Required(ErrorMessage = "GroupId is required")]
        public string group_name { get; set; }
    }
}
