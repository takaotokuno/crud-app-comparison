using AspNetNextApp.Api.Services.Products;
using Xunit;

namespace AspNetNextApp.Api.Tests.Services.Products
{
    public sealed class ProductServiceTests
    {
        [Fact]
        public async Task ListAsync_ReturnsEmptyPageUsingRequestedPaging()
        {
            ProductService service = new ProductService();
            ListProductsQuery query = new ListProductsQuery(
                Query: null,
                Status: null,
                Category: null,
                LowStock: null,
                SortBy: null,
                SortDirection: null,
                Page: 3,
                PageSize: 15);

            ProductResult<Contracts.Products.ProductListResponse> result = await service.ListAsync(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.Items);
            Assert.Equal(3, result.Value.Page);
            Assert.Equal(15, result.Value.PageSize);
            Assert.Equal(0, result.Value.TotalCount);
        }

        [Fact]
        public async Task GetAsync_ReturnsNotImplementedFailure()
        {
            ProductService service = new ProductService();

            ProductResult<Contracts.Products.ProductDetailResponse> result = await service.GetAsync(new GetProductQuery(Guid.NewGuid()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal("Not implemented.", result.Error);
        }
    }
}
