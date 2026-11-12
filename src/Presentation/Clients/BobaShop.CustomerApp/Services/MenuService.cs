using BobaShop.ClientNetworkLogic;
using HomeDtos = BobaShop.Application.Home;

namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Service thực đơn gọi Web API thật theo docs/api-contract-2026-11-09.md, dùng DTO của Application.
/// Lỗi thì ném InvalidOperationException kèm thông báo, trang gọi tự bắt và báo trên giao diện.
/// </summary>
public sealed class MenuService : IMenuService
{
    private readonly ApiClient _apiClient;

    public MenuService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.GetAsync<List<HomeDtos.CategoryDto>>("api/categories", ct: cancellationToken);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message);
        }

        return (result.Data ?? new List<HomeDtos.CategoryDto>())
            .Select(category => new MenuCategory(category.CategoryId, category.CategoryName))
            .ToList();
    }

    public Task<IReadOnlyList<MenuProduct>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return GetProductListAsync("api/products", cancellationToken);
    }

    public Task<IReadOnlyList<MenuProduct>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        return GetProductListAsync("api/products?featured=true", cancellationToken);
    }

    private async Task<IReadOnlyList<MenuProduct>> GetProductListAsync(string route, CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetAsync<List<HomeDtos.ProductDto>>(route, ct: cancellationToken);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message);
        }

        return (result.Data ?? new List<HomeDtos.ProductDto>())
            .Select(product => new MenuProduct(
                product.ProductId,
                product.CategoryId,
                product.ProductName,
                product.Price,
                product.ImageUrl))
            .ToList();
    }
}
