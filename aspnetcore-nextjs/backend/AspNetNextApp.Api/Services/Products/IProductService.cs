using AspNetNextApp.Api.Contracts.Products;

namespace AspNetNextApp.Api.Services.Products
{
    public interface IProductService
    {
        Task<ProductResult<ProductListResponse>> ListAsync(ListProductsQuery query, CancellationToken cancellationToken = default);

        Task<ProductResult<ProductDetailResponse>> GetAsync(GetProductQuery query, CancellationToken cancellationToken = default);

        Task<ProductResult<ProductDetailResponse>> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken = default);

        Task<ProductResult<ProductDetailResponse>> UpdateAsync(UpdateProductCommand command, CancellationToken cancellationToken = default);

        Task<ProductResult<bool>> DeleteAsync(DeleteProductCommand command, CancellationToken cancellationToken = default);
    }
}
