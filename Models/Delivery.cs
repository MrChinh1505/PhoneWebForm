using System.ComponentModel.DataAnnotations.Schema;

namespace MobilePhone.Models
{
    [Table("Delivery")]
    public class Delivery
    {
        [Column("deliveryID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int deliveryID { get; set; }
        public string? OrderID { get; set;}
        public string AgentID { get; set; }    
        public string? paymentStatus { get; set;}
        public string? deliverStatus { get; set;}
    }
}
