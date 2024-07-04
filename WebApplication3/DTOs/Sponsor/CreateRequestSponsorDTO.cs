using System.ComponentModel.DataAnnotations;

namespace WebApplication3.DTOs.Sponsor
{
    public class CreateRequestSponsorDTO
    {
        [Required(ErrorMessage = "Name is required")]
        public string name { get; set; }

        [Required(ErrorMessage = "Contact is required")]
        public string contact { get; set; }

        public string? address { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Contribution amount must be a positive number")]
        public int? contributionAmount { get; set; }
        public string nameProject { get; set; }
    }
}
