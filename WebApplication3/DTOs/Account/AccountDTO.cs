using System.Text.Json.Serialization;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.Member;
using WebApplication3.Entities;

namespace WebApplication3.DTOs.Account
{
    public class AccountDTO
    {

        public int Account_id { get; set; }
        public string Username { get; set; }
        [JsonIgnore]        
        public string Password { get; set; }

        public sbyte Status { get; set; } = 0;

        public string Role { get; set; } = "User";
        public MemberDTO Member { get; set; } = new MemberDTO();


    }
}
