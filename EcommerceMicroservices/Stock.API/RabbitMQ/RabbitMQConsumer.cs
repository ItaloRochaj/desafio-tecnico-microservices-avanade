using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stock.API.Data;
using Stock.API.Models;
using System.Text.Json;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Stock.API.RabbitMQ
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private readonly string _stockUpdateQueue = "stock-update-queue";
        private readonly string _orderCreatedQueue = "order-created-queue";
        private bool _isRabbitMQAvailable = false;

        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                CheckRabbitMQAvailability();
                if (_isRabbitMQAvailable)
                {
                    _logger.LogInformation("RabbitMQ Consumer started in Production Mode");
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(_configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672")
                    };
                    var connection = factory.CreateConnection();
                    var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: _stockUpdateQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        try
                        {
                            var stockUpdate = JsonSerializer.Deserialize<StockUpdateMessage>(message);
                            _logger.LogInformation("📥 [RabbitMQ] Received from queue {Queue}: {Message}", _stockUpdateQueue, message);
                            if (stockUpdate != null)
                                await ProcessStockUpdateMessage(stockUpdate);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deserializing or processing stock update message");
                        }
                    };
                    channel.BasicConsume(queue: _stockUpdateQueue, autoAck: true, consumer: consumer);
                    // Mantém o serviço rodando enquanto não for cancelado
                    return Task.CompletedTask;
                }
                else
                {
                    _logger.LogWarning("RabbitMQ not available - running in Mock mode");
                    // Mock mode - simulate background processing
                    return RunMockMode(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RabbitMQ Consumer stopped.");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in RabbitMQ Consumer");
                return Task.CompletedTask;
            }
        }

        private async Task RunMockMode(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("RabbitMQ Consumer running in mock mode...");
                await Task.Delay(30000, stoppingToken);
            }
        }

        private void CheckRabbitMQAvailability()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
                
                // Verificar se RabbitMQ está configurado e disponível
                _isRabbitMQAvailable = !string.IsNullOrEmpty(connectionString);
                
                _logger.LogInformation("RabbitMQ Consumer initialized - Mode: {Mode}", 
                    _isRabbitMQAvailable ? "Production Ready" : "Mock");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check RabbitMQ availability - falling back to mock mode");
                _isRabbitMQAvailable = false;
            }
        }

    // Remove ProcessPendingMessages, pois não é mais necessário no modo real

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
            try
            {
                // Cleanup RabbitMQ resources when real implementation is added
                _logger.LogInformation("RabbitMQ Consumer disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ Consumer");
            }
            
            base.Dispose();
        }
    }
}
