namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Bản giả để dựng trang chủ khi API chưa xong (theo mục 4 của Quy chuẩn).
/// Khi có API thật thì viết MenuService gọi ApiClient và đổi dòng khởi tạo trong Program.cs.
/// </summary>
public sealed class FakeMenuService : IMenuService
{
    private static readonly TimeSpan FakeNetworkDelay = TimeSpan.FromMilliseconds(500);

    private static readonly IReadOnlyList<MenuCategory> Categories = new[]
    {
        new MenuCategory(1, "Trà sữa"),
        new MenuCategory(2, "Trà trái cây"),
        new MenuCategory(3, "Cà phê"),
        new MenuCategory(4, "Đá xay"),
    };

    private static readonly IReadOnlyList<MenuProduct> Products = new[]
    {
        new MenuProduct(1, 1, "Trà sữa trân châu", 35000m, null),
        new MenuProduct(2, 1, "Trà sữa matcha", 40000m, null),
        new MenuProduct(3, 2, "Trà đào cam sả", 38000m, null),
        new MenuProduct(4, 2, "Trà vải", 36000m, null),
        new MenuProduct(5, 1, "Trà sữa khoai môn", 39000m, null),
        new MenuProduct(6, 1, "Trà sữa hoa nhài", 37000m, null),
        new MenuProduct(7, 2, "Trà chanh dây", 34000m, null),
        new MenuProduct(8, 3, "Cà phê sữa đá", 29000m, null),
        new MenuProduct(9, 3, "Bạc xỉu", 32000m, null),
        new MenuProduct(10, 4, "Matcha đá xay", 45000m, null),
        new MenuProduct(11, 4, "Sô cô la đá xay", 45000m, null),
        new MenuProduct(12, 1, "Sữa tươi trân châu đường đen", 42000m, null),
    };

    public async Task<IReadOnlyList<MenuCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(FakeNetworkDelay, cancellationToken);
        return Categories;
    }

    public async Task<IReadOnlyList<MenuProduct>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(FakeNetworkDelay, cancellationToken);
        return Products;
    }

    public async Task<IReadOnlyList<MenuProduct>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default)
    {
        // API thật hiện trả về toàn bộ món đang bán cho featured=true (ERD chưa có cờ nổi bật),
        // nên bản giả cũng làm vậy và trang chủ tự giới hạn số món hiển thị.
        await Task.Delay(FakeNetworkDelay, cancellationToken);
        return Products;
    }
}
