using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.DTOs.Project
{
    public class CreateProjectRequest
    {
        [Required(ErrorMessage = "Project name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Budget name is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be a positive number")]
        public decimal Budget { get; set; }

        [Column(TypeName = "TEXT")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
    }
}
