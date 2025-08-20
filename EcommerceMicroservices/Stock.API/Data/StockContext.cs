using Microsoft.EntityFrameworkCore;
using Stock.API.Models;

namespace Stock.API.Data;

public class StockContext : DbContext
{
    public StockContext(DbContextOptions<StockContext> options) : base(options) { }
    
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Notebook Dell", Description = "Notebook Dell Inspiron 15", Price = 4500.00m, QuantityInStock = 100 },
            new Product { Id = 2, Name = "Mouse Sem Fio", Description = "Mouse óptico sem fio", Price = 85.90m, QuantityInStock = 200 },
            new Product { Id = 3, Name = "Teclado Mecânico", Description = "Teclado mecânico RGB", Price = 320.00m, QuantityInStock = 75 }
        );
    }
}