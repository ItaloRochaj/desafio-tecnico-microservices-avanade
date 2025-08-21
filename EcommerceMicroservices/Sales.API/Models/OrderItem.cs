using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sales.API.Models;

[Table("OrderItems")]
public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    // Navigation Property
    public virtual Order Order { get; set; } = null!;

    // Calculated Property
    public decimal Subtotal => Quantity * UnitPrice;

    // Business Methods
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(newQuantity));
        
        Quantity = newQuantity;
    }

    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        if (newUnitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than 0", nameof(newUnitPrice));
        
        UnitPrice = newUnitPrice;
    }

    public void UpdateProduct(int productId, string productName, decimal unitPrice)
    {
        if (productId <= 0)
            throw new ArgumentException("Product ID must be greater than 0", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));
        
        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than 0", nameof(unitPrice));

        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
    }
}
