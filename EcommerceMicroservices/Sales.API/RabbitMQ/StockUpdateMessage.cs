namespace Sales.API.RabbitMQ
{
    public class StockUpdateMessage
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
