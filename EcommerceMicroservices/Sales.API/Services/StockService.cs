using System.Text.Json;

namespace Sales.API.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StockService> _logger;
    private readonly IConfiguration _configuration;

    public StockService(HttpClient httpClient, ILogger<StockService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> ValidateStockAsync(int productId, int quantity)
    {
        try
        {
            var product = await GetProductAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found during stock validation", productId);
                return false;
            }

            var hasStock = product.QuantityInStock >= quantity;
            _logger.LogInformation("Stock validation for Product {ProductId}: Requested={RequestedQty}, Available={AvailableQty}, Valid={IsValid}",
                productId, quantity, product.QuantityInStock, hasStock);

            return hasStock;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating stock for Product {ProductId}", productId);
            return false;
        }
    }

    public async Task<ProductDto?> GetProductAsync(int productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Product {ProductId} not found in Stock API", productId);
                    return null;
                }

                _logger.LogError("Failed to get product {ProductId} from Stock API. Status: {StatusCode}",
                    productId, response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogDebug("Successfully retrieved product {ProductId} from Stock API", productId);
            return product;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while getting product {ProductId} from Stock API", productId);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while getting product {ProductId} from Stock API", productId);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing product {ProductId} response from Stock API", productId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting product {ProductId} from Stock API", productId);
            return null;
        }
    }

    public async Task<bool> ReduceStockAsync(int productId, int quantity)
    {
        try
        {
            var requestBody = new
            {
                ProductId = productId,
                Quantity = quantity
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/products/{productId}/reduce-stock", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully reduced stock for Product {ProductId} by {Quantity}",
                    productId, quantity);
                return true;
            }

            _logger.LogWarning("Failed to reduce stock for Product {ProductId}. Status: {StatusCode}",
                productId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reducing stock for Product {ProductId}", productId);
            return false;
        }
    }
}
