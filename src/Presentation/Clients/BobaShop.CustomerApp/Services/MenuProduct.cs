namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Món trên trang chủ, theo cấu trúc trả về của GET /api/products?featured=true trong hợp đồng API.
/// TODO: thay bằng DTO trong Application khi Vy thêm.
/// </summary>
public sealed record MenuProduct(int ProductId, int CategoryId, string ProductName, decimal Price, string? ImageUrl);
