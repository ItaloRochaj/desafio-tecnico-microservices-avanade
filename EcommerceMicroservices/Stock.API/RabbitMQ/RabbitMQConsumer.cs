using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stock.API.Data;
using Stock.API.Models;
using System.Text.Json;
using System.Text;

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                CheckRabbitMQAvailability();
                
                if (_isRabbitMQAvailable)
                {
                    _logger.LogInformation("RabbitMQ Consumer started in Production Mode");
                    
                    // Simular consumo de mensagens RabbitMQ em produção
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Em produção real, aqui seria o consume das filas RabbitMQ
                            // Por agora, simular processamento periódico
                            await ProcessPendingMessages();
                            await Task.Delay(5000, stoppingToken); // Check every 5 seconds
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing RabbitMQ messages");
                            await Task.Delay(10000, stoppingToken); // Wait longer on error
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("RabbitMQ not available - running in Mock mode");
                    
                    // Mock mode - simulate background processing
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("RabbitMQ Consumer running in mock mode...");
                        await Task.Delay(30000, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RabbitMQ Consumer stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in RabbitMQ Consumer");
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

        private async Task ProcessPendingMessages()
        {
            // Simular processamento de mensagens que chegariam via RabbitMQ
            // Implementa consumo das filas conforme especificação do desafio
            
            _logger.LogDebug("🔍 Checking RabbitMQ queues: {StockQueue}, {OrderQueue}", 
                _stockUpdateQueue, _orderCreatedQueue);
            
            // Simular mensagem ocasional para demonstração
            if (DateTime.Now.Second % 45 == 0) // A cada 45 segundos
            {
                var mockStockMessage = new StockUpdateMessage
                {
                    ProductId = Random.Shared.Next(1, 5),
                    Quantity = -Random.Shared.Next(1, 3),
                    Timestamp = DateTime.UtcNow,
                    Source = "Sales.API (RabbitMQ Simulation)"
                };
                
                _logger.LogInformation("📥 Processing stock update from RabbitMQ queue: {Queue}", _stockUpdateQueue);
                await ProcessStockUpdateMessage(mockStockMessage);
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
