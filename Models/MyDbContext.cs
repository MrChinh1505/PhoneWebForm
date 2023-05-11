using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.EntityFrameworkCore;

namespace MobilePhone.Models
{
    public class MyDbContext : DbContext
    {

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Agent> Agents { get; set; }
        
        public virtual DbSet<Order> Orders { get; set; }

        public virtual DbSet<Delivery> Deliveries { get; set; }

        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
