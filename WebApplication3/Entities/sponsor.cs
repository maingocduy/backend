using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Entities
{
    public class sponsor
    {
        [Key, Column(Order = 1)]
        public int Sponsor_id { get; set; }
        
        public string name  { get; set; }
        public string contact { get; set; }

        public string address { get; set; }

        public int contributionAmount { get; set; }

        public DateTime time_create { get; set; }
    }
}
