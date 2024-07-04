using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication3.DTOs.Member;

namespace WebApplication3.Entities
{
    public class Project
    {
        [Key, Column(Order = 1)]

        public int Project_id { get; set; }

        public string Name { get; set; }
        public decimal Budget { get; set; }

        public decimal Contributions { get; set; }
        [Column(TypeName = "TEXT")]
        public string Description { get; set; }

        public sbyte Status { get; set; } = 0;
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; } 

        public List<MemberDTO>? MemberDTO { get; set;}
    }
}
