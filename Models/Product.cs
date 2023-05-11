using System.ComponentModel.DataAnnotations.Schema;

namespace MobilePhone.Models
{
    [Table("Product")]
    public class Product
    {
        [Column("ProductID")]
        public string? ProductID { get; set; }

        [Column("ProductName")]
        public string? ProductName { get; set; }

        [Column("Supplier")]
        public string? Supplier { get; set; }
        
        [Column("Price")]
        public int? Price { get; set; }

        [NotMapped]
        public int Quantity { get; set; }
    }
}
