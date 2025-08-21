using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sales.API.Data;

namespace Sales.API;

public class SalesContextFactory : IDesignTimeDbContextFactory<SalesContext>
{
    public SalesContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SalesContext>();
        
        // Connection string para design-time (migrations)
        var connectionString = "Server=localhost;Database=sales_db;User=developer;Password=Luke@2020;";
        
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        
        return new SalesContext(optionsBuilder.Options);
    }
}
