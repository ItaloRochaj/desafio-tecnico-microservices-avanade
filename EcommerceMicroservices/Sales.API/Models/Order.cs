using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sales.API.Models;

[Table("Orders")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Business Methods
    public void CalculateTotalAmount()
    {
        TotalAmount = OrderItems.Sum(item => item.Subtotal);
        UpdateTimestamp();
    }

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmOrder()
    {
        Status = "Confirmed";
        UpdateTimestamp();
    }

    public void CancelOrder()
    {
        Status = "Cancelled";
        UpdateTimestamp();
    }

    public bool CanBeModified()
    {
        return Status == "Pending";
    }

    public void AddItem(OrderItem item)
    {
        if (!CanBeModified())
            throw new InvalidOperationException("Cannot modify a confirmed or cancelled order");

        OrderItems.Add(item);
        CalculateTotalAmount();
    }

    public void RemoveItem(OrderItem item)
    {
        if (!CanBeModified())
            throw new InvalidOperationException("Cannot modify a confirmed or cancelled order");

        OrderItems.Remove(item);
        CalculateTotalAmount();
    }
}
