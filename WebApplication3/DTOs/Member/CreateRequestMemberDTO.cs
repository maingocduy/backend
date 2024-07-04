using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs.Member
{
    public class CreateRequestMemberDTO
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone is required")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Group_id is required")]
        public string Group_name { get; set; }

        public int Project_id { get; set; }
    }
}
