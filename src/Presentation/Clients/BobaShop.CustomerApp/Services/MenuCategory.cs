namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Danh mục đồ uống, theo cấu trúc trả về của GET /api/categories trong hợp đồng API.
/// TODO: thay bằng DTO trong Application khi Vy thêm.
/// </summary>
public sealed record MenuCategory(int CategoryId, string CategoryName);
