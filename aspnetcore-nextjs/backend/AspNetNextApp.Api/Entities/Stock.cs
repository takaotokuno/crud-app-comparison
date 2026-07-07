using System.ComponentModel.DataAnnotations;

using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Entities
{
    public sealed class Stock
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public Guid ProductId { get; private set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; private set; }

        [Range(0, int.MaxValue)]
        public int SafetyStock { get; private set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public Product Product { get; private set; } = null!;

        public ICollection<StockTransaction> StockTransactions { get; private set; } = [];

        private Stock()
        {
        }

        public static Stock Create(Product product, int initialQuantity, int safetyStock)
        {
            return new Stock
            {
                Product = product,
                ProductId = product.Id,
                Quantity = initialQuantity,
                SafetyStock = safetyStock,
            };
        }

        public void UpdateSafetyStock(int safetyStock)
        {
            SafetyStock = safetyStock;
        }

        public StockTransaction AdjustTo(int quantityAfter, string? reason = null, Guid? createdById = null)
        {
            return ApplyTransaction(StockTransactionType.Adjustment, quantityAfter - Quantity, reason, createdById);
        }

        public StockTransaction ApplyTransaction(StockTransactionType type, int quantityDelta, string? reason = null, Guid? createdById = null)
        {
            int quantityAfter = Quantity + quantityDelta;
            StockTransaction transaction = StockTransaction.Create(this, type, quantityDelta, quantityAfter, reason, createdById);
            Quantity = quantityAfter;

            return transaction;
        }
    }
}
