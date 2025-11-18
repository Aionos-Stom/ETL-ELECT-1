using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ProcesoETL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .Property(c => c.CustomerID)
            .ValueGeneratedNever();

        modelBuilder.Entity<Product>()
            .Property(p => p.ProductID)
            .ValueGeneratedNever();

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderID)
            .ValueGeneratedNever();

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.OrderDetailID)
            .ValueGeneratedNever();

        base.OnModelCreating(modelBuilder);
    }
}
