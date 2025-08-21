using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.API.Data;
using Sales.API.Models;
using Sales.API.Services;
using Sales.API.RabbitMQ;
using Sales.API.DTOs;

namespace Sales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly SalesContext _context;
    private readonly IStockService _stockService;
    private readonly RabbitMQPublisher _rabbitMQPublisher;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        SalesContext context,
        IStockService stockService,
        RabbitMQPublisher rabbitMQPublisher,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _stockService = stockService;
        _rabbitMQPublisher = rabbitMQPublisher;
        _logger = logger;
    }

    // GET: api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderDTOs = orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                CustomerId = 0, // Order não tem CustomerId, usando 0 como padrão
                CustomerName = o.CustomerName,
                CustomerEmail = o.CustomerEmail,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice // Calculando TotalPrice
                }).ToList()
            }).ToList();

            return Ok(orderDTOs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders");
            return StatusCode(500, "Internal server error while retrieving orders");
        }
    }

    // GET: api/orders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "Internal server error while retrieving order");
        }
    }

    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            // Validar dados de entrada
            if (string.IsNullOrWhiteSpace(request.CustomerName) || 
                string.IsNullOrWhiteSpace(request.CustomerEmail) ||
                request.Items == null || !request.Items.Any())
            {
                return BadRequest("Customer name, email, and items are required");
            }

            // Validar estoque para todos os itens
            foreach (var item in request.Items)
            {
                var hasStock = await _stockService.ValidateStockAsync(item.ProductId, item.Quantity);
                if (!hasStock)
                {
                    return BadRequest($"Insufficient stock for product {item.ProductId}");
                }

                // Obter dados do produto
                var product = await _stockService.GetProductAsync(item.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product {item.ProductId} not found");
                }

                // Atualizar nome e preço do produto
                item.ProductName = product.Name;
                item.UnitPrice = product.Price;
            }

            // Criar o pedido
            var order = new Order
            {
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            // Adicionar itens ao pedido
            foreach (var item in request.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                order.OrderItems.Add(orderItem);
            }

            // Calcular total
            order.CalculateTotalAmount();

            // Salvar no banco de dados
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Reduzir estoque para cada item
            foreach (var item in order.OrderItems)
            {
                await _stockService.ReduceStockAsync(item.ProductId, item.Quantity);
                await _rabbitMQPublisher.PublishStockUpdateAsync(item.ProductId, item.Quantity);
            }

            // Publicar evento de pedido criado
            await _rabbitMQPublisher.PublishOrderCreatedAsync(order);

            _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerName}",
                order.Id, order.CustomerName);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerName}", request.CustomerName);
            return StatusCode(500, "Internal server error while creating order");
        }
    }

    // PUT: api/orders/5/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            var oldStatus = order.Status;
            
            switch (request.Status.ToLower())
            {
                case "confirmed":
                    order.ConfirmOrder();
                    break;
                case "cancelled":
                    order.CancelOrder();
                    break;
                default:
                    return BadRequest("Invalid status. Valid values: confirmed, cancelled");
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus}",
                id, oldStatus, order.Status);

            return Ok(new { Message = $"Order status updated to {order.Status}", Order = order });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for order {OrderId}", id);
            return StatusCode(500, "Internal server error while updating order status");
        }
    }

    // DELETE: api/orders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            if (order.Status == "Confirmed")
            {
                return BadRequest("Cannot delete a confirmed order");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} deleted successfully", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            return StatusCode(500, "Internal server error while deleting order");
        }
    }
}

// DTOs para as requisições
public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}