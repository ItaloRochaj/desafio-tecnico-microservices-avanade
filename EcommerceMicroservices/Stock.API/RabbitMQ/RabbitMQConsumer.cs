using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stock.API.Data;
using Stock.API.Models;
using System.Text.Json;

namespace Stock.API.RabbitMQ
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Consumer started (Mock implementation for development)");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // In a real implementation, this would consume messages from RabbitMQ
                    // For development purposes, this is a mock consumer that logs activity
                    
                    _logger.LogDebug("RabbitMQ Consumer is running...");
                    
                    // Simulate consuming messages every 30 seconds (in production this would be event-driven)
                    await Task.Delay(30000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("RabbitMQ Consumer stopped.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RabbitMQ Consumer");
                    // Wait before retrying
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        /// <summary>
        /// Mock method to simulate processing stock update messages
        /// In a real implementation, this would be called when messages are received
        /// </summary>
        public async Task ProcessStockUpdateMessage(StockUpdateMessage message)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StockContext>();

                _logger.LogInformation("Processing stock update message for Product ID: {ProductId}, Quantity: {Quantity}", 
                    message.ProductId, message.Quantity);

                var product = await context.Products.FindAsync(message.ProductId);
                if (product != null)
                {
                    var previousStock = product.QuantityInStock;
                    
                    if (message.Quantity > 0)
                    {
                        product.IncreaseStock(message.Quantity);
                    }
                    else if (message.Quantity < 0)
                    {
                        product.ReduceStock(Math.Abs(message.Quantity));
                    }
                    
                    await context.SaveChangesAsync();

                    _logger.LogInformation("Stock updated for Product {ProductId}: {PreviousStock} -> {NewStock}", 
                        message.ProductId, previousStock, product.QuantityInStock);
                }
                else
                {
                    _logger.LogWarning("Product not found for stock update: {ProductId}", message.ProductId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock update message for Product ID: {ProductId}", message.ProductId);
            }
        }

        public override void Dispose()
        {
            _logger.LogInformation("RabbitMQ Consumer disposed");
            base.Dispose();
        }
    }
}
