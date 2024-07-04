using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs.Sponsor
{
    public class UpdateRequestSponsorDTO
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string contact { get; set; }

        public string address { get; set; }
    }
}
