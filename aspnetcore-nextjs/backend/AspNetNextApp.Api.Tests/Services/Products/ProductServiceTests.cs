using AspNetNextApp.Api.Services.Products;

namespace AspNetNextApp.Api.Tests.Services.Products;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task ListAsync_ReturnsEmptyPageUsingRequestedPaging()
    {
        var service = new ProductService();
        var query = new ListProductsQuery(
            Query: null,
            Status: null,
            Category: null,
            LowStock: null,
            SortBy: null,
            SortDirection: null,
            Page: 3,
            PageSize: 15);

        var result = await service.ListAsync(query, CancellationToken.None);

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
        var service = new ProductService();

        var result = await service.GetAsync(new GetProductQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("Not implemented.", result.Error);
    }
}
