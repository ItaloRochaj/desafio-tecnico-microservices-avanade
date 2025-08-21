namespace Sales.API.Services;

public interface IStockService
{
    Task<bool> ValidateStockAsync(int productId, int quantity);
    Task<ProductDto?> GetProductAsync(int productId);
    Task<bool> ReduceStockAsync(int productId, int quantity);
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int QuantityInStock { get; set; }
}
