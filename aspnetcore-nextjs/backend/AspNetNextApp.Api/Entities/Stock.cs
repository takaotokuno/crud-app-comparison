using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Entities;

public sealed class Stock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]
    public int SafetyStock { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;

    public ICollection<StockTransaction> StockTransactions { get; set; } = [];
}
