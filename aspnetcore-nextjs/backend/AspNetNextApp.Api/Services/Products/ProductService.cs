using AspNetNextApp.Api.Contracts.Products;

namespace AspNetNextApp.Api.Services.Products;

public sealed class ProductService : IProductService
{
    public Task<ProductResult<ProductListResponse>> ListAsync(ListProductsQuery query, CancellationToken cancellationToken = default)
    {
        var response = new ProductListResponse([], query.Page, query.PageSize, 0);
        return Task.FromResult(ProductResult<ProductListResponse>.Success(response));
    }

    public Task<ProductResult<ProductDetailResponse>> GetAsync(GetProductQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ProductResult<ProductDetailResponse>.Failure("Not implemented."));
    }

    public Task<ProductResult<ProductDetailResponse>> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ProductResult<ProductDetailResponse>.Failure("Not implemented."));
    }

    public Task<ProductResult<ProductDetailResponse>> UpdateAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ProductResult<ProductDetailResponse>.Failure("Not implemented."));
    }

    public Task<ProductResult<bool>> DeleteAsync(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ProductResult<bool>.Failure("Not implemented."));
    }
}
