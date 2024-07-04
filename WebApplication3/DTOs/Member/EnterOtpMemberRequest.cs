using WebApplication3.DTOs.Otp;

namespace WebApplication3.DTOs.Member
{
    public class EnterOtpMemberRequest : otpRequest
    {
        public int Project_id { get; set; }
        public string Email { get; set; }
    }
}
