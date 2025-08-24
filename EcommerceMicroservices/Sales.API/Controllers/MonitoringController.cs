using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sales.API.RabbitMQ;

namespace Sales.API.Controllers
{
    /// <summary>
    /// Controller para monitoramento do sistema conforme extras do desafio
    /// Implementa "Monitoramento e Logs" para rastrear falhas e transações
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MonitoringController : ControllerBase
    {
        private readonly ILogger<MonitoringController> _logger;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public MonitoringController(ILogger<MonitoringController> logger, RabbitMQPublisher rabbitMQPublisher)
        {
            _logger = logger;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        /// <summary>
        /// Status de saúde do microserviço de vendas
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult GetHealthStatus()
        {
            try
            {
                var healthStatus = new
                {
                    Service = "Sales.API",
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    RabbitMQ = new
                    {
                        Status = _rabbitMQPublisher.GetConnectionStatus(),
                        IsConnected = _rabbitMQPublisher.IsConnectedToRabbitMQ
                    },
                    Features = new
                    {
                        OrderCreation = "Active",
                        StockValidation = "Active",
                        RabbitMQMessaging = _rabbitMQPublisher.IsConnectedToRabbitMQ ? "Production" : "Mock",
                        JWTAuthentication = "Active"
                    }
                };

                _logger.LogInformation("Health check requested - System healthy");
                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
            }
        }

        /// <summary>
        /// Estatísticas do sistema para monitoramento
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetSystemStats()
        {
            try
            {
                var stats = new
                {
                    Service = "Sales.API",
                    Uptime = DateTime.UtcNow,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                    MachineName = Environment.MachineName,
                    Architecture = new
                    {
                        Microservice = "Sales Management",
                        Communication = "HTTP + RabbitMQ Async",
                        Database = "MySQL with Entity Framework",
                        Authentication = "JWT",
                        ApiGateway = "Integrated"
                    }
                };

                _logger.LogInformation("System statistics requested");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system statistics");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Teste de conectividade RabbitMQ
        /// </summary>
        [HttpPost("test-rabbitmq")]
        public async Task<IActionResult> TestRabbitMQConnectivity()
        {
            try
            {
                _logger.LogInformation("RabbitMQ connectivity test initiated");
                
                // Simular teste de mensagem
                await _rabbitMQPublisher.PublishStockUpdateAsync(999, 1);
                
                var result = new
                {
                    Test = "RabbitMQ Connectivity",
                    Status = _rabbitMQPublisher.IsConnectedToRabbitMQ ? "Connected" : "Mock Mode",
                    ConnectionStatus = _rabbitMQPublisher.GetConnectionStatus(),
                    TestMessage = "Stock update test message sent",
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("RabbitMQ test completed successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ test failed");
                return StatusCode(500, new { Error = ex.Message, Test = "Failed" });
            }
        }
    }
}
