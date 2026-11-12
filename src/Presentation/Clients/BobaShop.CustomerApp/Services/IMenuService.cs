namespace BobaShop.CustomerApp.Services;

/// <summary>Service thực đơn phía client. Form chỉ gọi interface này, không gọi ApiClient hay HttpClient.</summary>
public interface IMenuService
{
    Task<IReadOnlyList<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>Toàn bộ món đang bán, hiển thị ở trang Thực đơn.</summary>
    Task<IReadOnlyList<MenuProduct>> GetProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>Danh sách món hiển thị ở khu vực nổi bật của trang chủ.</summary>
    Task<IReadOnlyList<MenuProduct>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default);
}
