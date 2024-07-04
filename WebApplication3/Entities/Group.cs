using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Entities
{
    public class Group
    {
        [Key, Column(Order = 1)]
        public int Groups_id { get; set; }

        public string group_name { get; set; }
    }
}
