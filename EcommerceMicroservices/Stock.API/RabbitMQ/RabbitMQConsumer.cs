using Microsoft.Extensions.Logging;

namespace Stock.API.RabbitMQ;

public class RabbitMQConsumer
{
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger)
    {
        _logger = logger;
    }

    public void StartConsuming()
    {
        _logger.LogInformation("RabbitMQ Consumer desabilitado para testes dos endpoints.");
        // Consumer desabilitado para testes
    }
}
