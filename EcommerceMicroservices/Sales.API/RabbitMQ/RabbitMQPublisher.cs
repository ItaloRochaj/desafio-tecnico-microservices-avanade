using Microsoft.Extensions.Logging;
using Sales.API.Models;
using System.Text.Json;

namespace Sales.API.RabbitMQ
{
    public class RabbitMQPublisher
    {
        private readonly ILogger<RabbitMQPublisher> _logger;

        public RabbitMQPublisher(ILogger<RabbitMQPublisher> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Publishes a stock update message to RabbitMQ
        /// This is a mock implementation for development purposes
        /// </summary>
        /// <param name="productId">The ID of the product to update stock for</param>
        /// <param name="quantityReduction">The quantity to reduce from stock (positive number)</param>
        public async Task PublishStockUpdateAsync(int productId, int quantityReduction)
        {
            try
            {
                var message = new StockUpdateMessage
                {
                    ProductId = productId,
                    Quantity = -quantityReduction, // Negative for reduction
                    Timestamp = DateTime.UtcNow,
                    Source = "Sales.API"
                };

                // In a real implementation, this would publish to RabbitMQ
                // For development purposes, we'll just log the message
                _logger.LogInformation("Stock update message published (Mock): {Message}", 
                    JsonSerializer.Serialize(message));

                // Simulate async operation
                await Task.Delay(10);

                _logger.LogDebug("Successfully published stock update for Product {ProductId}, Quantity: {Quantity}", 
                    productId, quantityReduction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish stock update message for Product {ProductId}", productId);
                throw;
            }
        }

        /// <summary>
        /// Publishes an order created event to RabbitMQ
        /// This is a mock implementation for development purposes
        /// </summary>
        /// <param name="order">The order that was created</param>
        public async Task PublishOrderCreatedAsync(Order order)
        {
            try
            {
                var message = new
                {
                    OrderId = order.Id,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    Items = order.OrderItems.Select(item => new
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal
                    })
                };

                // In a real implementation, this would publish to RabbitMQ
                // For development purposes, we'll just log the message
                _logger.LogInformation("Order created event published (Mock): {Message}", 
                    JsonSerializer.Serialize(message));

                // Simulate async operation
                await Task.Delay(10);

                _logger.LogDebug("Successfully published order created event for Order {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish order created event for Order {OrderId}", order.Id);
                throw;
            }
        }
    }
}
