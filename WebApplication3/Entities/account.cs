using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication3.DTOs.Member;

namespace WebApplication3.Entities
{
    public class account
    {
        [Key, Column(Order = 1)]

        public int Account_id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

    }
}
