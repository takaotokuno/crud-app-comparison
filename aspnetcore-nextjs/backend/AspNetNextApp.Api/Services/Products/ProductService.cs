using AspNetNextApp.Api.Contracts.Products;
using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Services.Products
{
    public sealed class ProductService(AppDbContext dbContext) : IProductService
    {
        private const int MaxPageSize = 100;

        public async Task<ProductResult<ProductListResponse>> ListAsync(ListProductsQuery query, CancellationToken cancellationToken = default)
        {
            int page = Math.Max(query.Page, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            IQueryable<Product> products = dbContext.Products
                .AsNoTracking()
                .Include(product => product.Stock)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                string keyword = query.Query.Trim();
                products = products.Where(product =>
                    product.Sku.Contains(keyword) ||
                    product.Name.Contains(keyword) ||
                    (product.Description != null && product.Description.Contains(keyword)));
            }

            if (query.Status.HasValue)
            {
                products = products.Where(product => product.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                string category = query.Category.Trim();
                products = products.Where(product => product.Category == category);
            }

            if (query.LowStock == true)
            {
                products = products.Where(product => product.Stock != null && product.Stock.Quantity <= product.Stock.SafetyStock);
            }

            int totalCount = await products.CountAsync(cancellationToken);
            List<ProductSummaryResponse> items = await ApplySort(products, query.SortBy, query.SortDirection)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(product => new ProductSummaryResponse(
                    product.Id,
                    product.Sku,
                    product.Name,
                    product.Category,
                    product.Price,
                    product.Status,
                    product.Stock == null ? 0 : product.Stock.Quantity,
                    product.Stock == null ? 0 : product.Stock.SafetyStock,
                    product.UpdatedAt))
                .ToListAsync(cancellationToken);

            return ProductResult<ProductListResponse>.Success(new ProductListResponse(items, page, pageSize, totalCount));
        }

        public async Task<ProductResult<ProductDetailResponse>> GetAsync(GetProductQuery query, CancellationToken cancellationToken = default)
        {
            Product? product = await FindProductWithStockAsync(query.Id, cancellationToken);

            return product is null
                ? ProductResult<ProductDetailResponse>.Failure("Product was not found.", ProductErrorType.NotFound)
                : ProductResult<ProductDetailResponse>.Success(ToDetailResponse(product));
        }

        public async Task<ProductResult<ProductDetailResponse>> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
        {
            string sku = command.Sku.Trim();
            if (await IsSkuInUseAsync(sku, excludedProductId: null, cancellationToken))
            {
                return ProductResult<ProductDetailResponse>.Failure("SKU is already in use.", ProductErrorType.Conflict);
            }

            Product product = Product.Create(
                sku,
                command.Name,
                command.Description,
                command.Category,
                command.Price,
                command.Status,
                command.InitialQuantity,
                command.SafetyStock);

            _ = dbContext.Products.Add(product);
            _ = await dbContext.SaveChangesAsync(cancellationToken);

            return ProductResult<ProductDetailResponse>.Success(ToDetailResponse(product));
        }

        public async Task<ProductResult<ProductDetailResponse>> UpdateAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
        {
            Product? product = await FindProductWithStockAsync(command.Id, cancellationToken);
            if (product is null)
            {
                return ProductResult<ProductDetailResponse>.Failure("Product was not found.", ProductErrorType.NotFound);
            }

            string sku = command.Sku.Trim();
            if (await IsSkuInUseAsync(sku, command.Id, cancellationToken))
            {
                return ProductResult<ProductDetailResponse>.Failure("SKU is already in use.", ProductErrorType.Conflict);
            }

            product.UpdateDetails(sku, command.Name, command.Description, command.Category, command.Price, command.Status);
            _ = await dbContext.SaveChangesAsync(cancellationToken);

            return ProductResult<ProductDetailResponse>.Success(ToDetailResponse(product));
        }

        public async Task<ProductResult<bool>> DeleteAsync(DeleteProductCommand command, CancellationToken cancellationToken = default)
        {
            Product? product = await dbContext.Products.FindAsync([command.Id], cancellationToken);
            if (product is null)
            {
                return ProductResult<bool>.Failure("Product was not found.", ProductErrorType.NotFound);
            }

            product.Discontinue();
            _ = await dbContext.SaveChangesAsync(cancellationToken);

            return ProductResult<bool>.Success(true);
        }

        private static IQueryable<Product> ApplySort(IQueryable<Product> products, string? sortBy, string? sortDirection)
        {
            bool descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "sku" => descending ? products.OrderByDescending(product => product.Sku) : products.OrderBy(product => product.Sku),
                "name" => descending ? products.OrderByDescending(product => product.Name) : products.OrderBy(product => product.Name),
                "price" => descending ? products.OrderByDescending(product => product.Price) : products.OrderBy(product => product.Price),
                "quantity" => descending
                    ? products.OrderByDescending(product => product.Stock == null ? 0 : product.Stock.Quantity)
                    : products.OrderBy(product => product.Stock == null ? 0 : product.Stock.Quantity),
                "created_at" => descending ? products.OrderByDescending(product => product.CreatedAt) : products.OrderBy(product => product.CreatedAt),
                "updated_at" or _ => descending || string.IsNullOrWhiteSpace(sortDirection)
                    ? products.OrderByDescending(product => product.UpdatedAt)
                    : products.OrderBy(product => product.UpdatedAt),
            };
        }

        private Task<Product?> FindProductWithStockAsync(Guid id, CancellationToken cancellationToken)
        {
            return dbContext.Products
                .Include(product => product.Stock)
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        }

        private Task<bool> IsSkuInUseAsync(string sku, Guid? excludedProductId, CancellationToken cancellationToken)
        {
            return dbContext.Products.AnyAsync(
                product => product.Sku == sku && (!excludedProductId.HasValue || product.Id != excludedProductId.Value),
                cancellationToken);
        }

        private static ProductDetailResponse ToDetailResponse(Product product)
        {
            return new(
                product.Id,
                product.Sku,
                product.Name,
                product.Description,
                product.Category,
                product.Price,
                product.Status,
                product.Stock?.Quantity ?? 0,
                product.Stock?.SafetyStock ?? 0,
                product.CreatedAt,
                product.UpdatedAt);
        }
    }
}
