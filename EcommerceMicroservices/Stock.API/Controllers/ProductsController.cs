using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock.API.Data;
using Stock.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace Stock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly StockContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(StockContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        try
        {
            _logger.LogInformation("Consultando todos os produtos");
            return await _context.Products.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar produtos");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        try
        {
            _logger.LogInformation("Consultando produto com ID: {ProductId}", id);
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado", id);
                return NotFound();
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar produto com ID: {ProductId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        try
        {
            _logger.LogInformation("Criando novo produto: {ProductName}", product.Name);
            
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Produto criado com ID: {ProductId}", product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto: {ProductName}", product.Name);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantity)
    {
        try
        {
            _logger.LogInformation("Atualizando estoque do produto ID: {ProductId}, quantidade: {Quantity}", id, quantity);
            
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado para atualização de estoque", id);
                return NotFound();
            }
            
            product.QuantityInStock += quantity;
            product.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Estoque do produto ID: {ProductId} atualizado para {NewQuantity}", id, product.QuantityInStock);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar estoque do produto ID: {ProductId}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateStock([FromBody] StockValidationRequest request)
    {
        try
        {
            _logger.LogInformation("Validando estoque para o produto ID: {ProductId}, quantidade: {Quantity}", request.ProductId, request.Quantity);
            
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado para validação", request.ProductId);
                return Ok(new { IsValid = false, Message = "Produto não encontrado" });
            }

            var isValid = product.QuantityInStock >= request.Quantity;
            
            if (!isValid)
            {
                _logger.LogWarning("Estoque insuficiente para o produto ID: {ProductId}. Disponível: {Available}, Solicitado: {Requested}", 
                    request.ProductId, product.QuantityInStock, request.Quantity);
            }
            else
            {
                _logger.LogInformation("Estoque validado com sucesso para o produto ID: {ProductId}", request.ProductId);
            }

            return Ok(new { IsValid = isValid, Available = product.QuantityInStock });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar estoque para o produto ID: {ProductId}", request.ProductId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}

public class StockValidationRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}