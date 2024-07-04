using System.Text.Json.Serialization;
using WebApplication3.DTOs.Groups;
using WebApplication3.Entities;

namespace WebApplication3.DTOs.Member
{
    public class MemberDTO
    {
        
        public int Member_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
     
        public Group groups { get; set;}
    }
}
