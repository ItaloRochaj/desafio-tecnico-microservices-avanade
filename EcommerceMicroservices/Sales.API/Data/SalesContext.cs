using Microsoft.EntityFrameworkCore;
using Sales.API.Models;

namespace Sales.API.Data;

public class SalesContext : DbContext
{
    public SalesContext(DbContextOptions<SalesContext> options) : base(options) { }
    
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração da entidade Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
            // Configurar relacionamento 1:N com OrderItems
            entity.HasMany(e => e.OrderItems)
                  .WithOne(e => e.Order)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.OrderId).IsRequired();
        });

        // Seed Data para testes
        var order1 = new Order
        {
            Id = 1,
            CustomerName = "João Silva",
            CustomerEmail = "joao.silva@email.com",
            OrderDate = DateTime.UtcNow.AddDays(-1),
            Status = "Confirmed",
            TotalAmount = 4585.90m,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var order2 = new Order
        {
            Id = 2,
            CustomerName = "Maria Santos",
            CustomerEmail = "maria.santos@email.com",
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            TotalAmount = 405.90m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<Order>().HasData(order1, order2);

        modelBuilder.Entity<OrderItem>().HasData(
            new OrderItem
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1,
                ProductName = "Notebook Dell",
                Quantity = 1,
                UnitPrice = 4500.00m
            },
            new OrderItem
            {
                Id = 2,
                OrderId = 1,
                ProductId = 2,
                ProductName = "Mouse Sem Fio",
                Quantity = 1,
                UnitPrice = 85.90m
            },
            new OrderItem
            {
                Id = 3,
                OrderId = 2,
                ProductId = 3,
                ProductName = "Teclado Mecânico",
                Quantity = 1,
                UnitPrice = 320.00m
            },
            new OrderItem
            {
                Id = 4,
                OrderId = 2,
                ProductId = 2,
                ProductName = "Mouse Sem Fio",
                Quantity = 1,
                UnitPrice = 85.90m
            }
        );
    }
}