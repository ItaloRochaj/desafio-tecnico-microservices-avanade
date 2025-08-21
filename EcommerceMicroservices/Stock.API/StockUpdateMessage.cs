namespace Stock.API.RabbitMQ;

public class StockUpdateMessage
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Operation { get; set; } = "REDUCE"; // REDUCE, INCREASE
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
}