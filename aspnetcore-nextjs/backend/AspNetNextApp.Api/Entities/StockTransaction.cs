using System.ComponentModel.DataAnnotations;

using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Entities
{
    public sealed class StockTransaction : IValidatableObject
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid ProductId { get; private set; }

        public Guid StockId { get; private set; }

        public StockTransactionType Type { get; private set; }

        public int QuantityDelta { get; private set; }

        [Range(0, int.MaxValue)]
        public int QuantityAfter { get; private set; }

        [MaxLength(255)]
        public string? Reason { get; private set; }

        public Guid? CreatedById { get; private set; }

        public DateTimeOffset CreatedAt { get; set; }

        public Product Product { get; private set; } = null!;

        public Stock Stock { get; private set; } = null!;

        public User? CreatedBy { get; private set; }

        private StockTransaction()
        {
        }

        public static StockTransaction Create(
            Stock stock,
            StockTransactionType type,
            int quantityDelta,
            int quantityAfter,
            string? reason = null,
            Guid? createdById = null)
        {
            StockTransaction transaction = new()
            {
                Product = stock.Product,
                ProductId = stock.ProductId,
                Stock = stock,
                StockId = stock.Id,
                Type = type,
                QuantityDelta = quantityDelta,
                QuantityAfter = quantityAfter,
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                CreatedById = createdById,
            };
            Validator.ValidateObject(transaction, new ValidationContext(transaction), validateAllProperties: true);

            return transaction;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(Type))
            {
                yield return new ValidationResult(
                    "Type must be a defined stock transaction type.",
                    [nameof(Type)]);
            }

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
}
