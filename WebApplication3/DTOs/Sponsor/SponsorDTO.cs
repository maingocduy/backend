using System.Text.Json.Serialization;

namespace WebApplication3.DTOs.Sponsor
{
    public class SponsorDTO
    {
        [JsonIgnore]
        public int Sponsor_id { get; set; }
        
        public string name { get; set; }
        public string contact { get; set; }

        public string address { get; set; }

        public int contributionAmount { get; set; }

        public DateTime time_create { get; set; }
    }
}
