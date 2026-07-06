using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Entities;

public sealed class StockTransaction : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }

    public Guid StockId { get; set; }

    public StockTransactionType Type { get; set; }

    public int QuantityDelta { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityAfter { get; set; }

    [MaxLength(255)]
    public string? Reason { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Product Product { get; set; } = null!;

    public Stock Stock { get; set; } = null!;

    public User? CreatedBy { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (QuantityDelta == 0)
        {
            yield return new ValidationResult(
                "Quantity delta must not be zero.",
                [nameof(QuantityDelta)]);
        }

        if (Type == StockTransactionType.Inbound && QuantityDelta < 0)
        {
            yield return new ValidationResult(
                "Inbound transactions must increase stock quantity.",
                [nameof(QuantityDelta)]);
        }

        if (Type == StockTransactionType.Outbound && QuantityDelta > 0)
        {
            yield return new ValidationResult(
                "Outbound transactions must decrease stock quantity.",
                [nameof(QuantityDelta)]);
        }
    }
}
