using System.ComponentModel.DataAnnotations.Schema;

namespace MobilePhone.Models
{
    [Table("OrderTable")]   
    
    public class Order
    {
        [Column("OrderID")]
        public string? OrderID { get; set; }
        
        [Column("DateOrdered")]
        public string? OrderDate { get; set; }
        
        [Column("AgentID")]
        public string? AgentID { get; set; }
        
        [Column("Total")]
        public int? Total { get; set; }
    }
}
