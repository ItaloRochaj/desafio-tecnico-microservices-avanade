using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stock.API.Models;

[Table("Products")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int QuantityInStock { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Método para atualizar o timestamp quando o produto for modificado
    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    // Método para validar se há estoque suficiente
    public bool HasSufficientStock(int requestedQuantity)
    {
        return QuantityInStock >= requestedQuantity && requestedQuantity > 0;
    }

    // Método para reduzir o estoque
    public bool ReduceStock(int quantity)
    {
        if (!HasSufficientStock(quantity))
            return false;

        QuantityInStock -= quantity;
        UpdateTimestamp();
        return true;
    }

    // Método para aumentar o estoque
    public void IncreaseStock(int quantity)
    {
        if (quantity > 0)
        {
            QuantityInStock += quantity;
            UpdateTimestamp();
        }
    }
}