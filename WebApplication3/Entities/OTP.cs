using CloudinaryDotNet;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Entities
{
    public class OTP
    {
        [Key, Column(Order = 1)]
            public int Otp_Id { get; set; }

            public string OtpCode { get; set; }


            public DateTime CreatedAt { get; set; }

            public DateTime ExpiresAt { get; set; }

    }
}
