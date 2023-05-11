using System.ComponentModel.DataAnnotations.Schema;

namespace MobilePhone.Models
{
    [Table("Agent")]
    public class Agent
    {
        [Column("AgentID")]
        public string id { get; set; }
       
        [Column("AgentName")]
        public string? name { get; set; }
        
        [Column("Address")]
        public string? address { get; set; }
        
        [Column("Contact")]
        public string? contact { get; set; }

        [Column("Password")]
        public string pwd { get; set; }
    }
}
