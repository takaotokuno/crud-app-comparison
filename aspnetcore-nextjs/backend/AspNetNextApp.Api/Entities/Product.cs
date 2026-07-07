using System.ComponentModel.DataAnnotations;

using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Entities;

public sealed class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [RegularExpression(@"^[A-Za-z0-9_-]{1,32}$")]
    [MaxLength(32)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [Range(0, int.MaxValue)]
    public int Price { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Stock? Stock { get; set; }

    public ICollection<StockTransaction> StockTransactions { get; set; } = [];
}
