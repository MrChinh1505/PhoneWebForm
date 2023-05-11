using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MobilePhone.Models
{
    [Table("OrderDetail")]
    public class OrderDetail
    {
        [Key]
        [Column("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("OrderID")]
        public string? OrderID { get; set; }

        [Column("ProductID")]
        public string? ProductID { get; set; }

        [Column("Quan")]
        public int? Quantity { get; set; }

        [Column("Price")]
        public int? Total { get; set; }
    }
}
