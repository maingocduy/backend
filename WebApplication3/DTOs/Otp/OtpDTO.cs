using System.Text.Json.Serialization;

namespace WebApplication3.DTOs.Otp
{
    public class OtpDTO
    {
        [JsonIgnore]
        public int id { get; set; }

        public string otp_code { get; set; }


        public DateTime created_at { get; set; }

        public DateTime expires_at { get; set; }

        public bool IsVerified { get; set; } = false;
    }
}
