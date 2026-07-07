using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Entities;

public sealed class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    [RegularExpression(@"^[A-Za-z0-9_-]{1,32}$")]
    [MaxLength(32)]
    public string Sku { get; private set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; private set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; private set; }

    [MaxLength(50)]
    public string? Category { get; private set; }

    [Range(0, int.MaxValue)]
    public int Price { get; private set; }

    public ProductStatus Status { get; private set; } = ProductStatus.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Stock? Stock { get; private set; }

    public ICollection<StockTransaction> StockTransactions { get; private set; } = [];

    private Product()
    {
    }

    public static Product Create(
        string sku,
        string name,
        string? description,
        string? category,
        int price,
        ProductStatus status,
        int initialQuantity,
        int safetyStock)
    {
        var product = new Product();
        product.UpdateDetails(sku, name, description, category, price, status);
        product.Stock = Stock.Create(product, initialQuantity, safetyStock);

        return product;
    }

    public void UpdateDetails(
        string sku,
        string name,
        string? description,
        string? category,
        int price,
        ProductStatus status)
    {
        Sku = sku.Trim();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        Price = price;
        Status = status;
    }
}
