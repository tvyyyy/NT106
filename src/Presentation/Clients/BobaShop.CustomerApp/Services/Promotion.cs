namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Một chương trình khuyến mãi hiển thị trên trang chủ.
/// Lấy từ GET /api/promotions; bản giả dùng dữ liệu mẫu cùng cấu trúc.
/// </summary>
public sealed record Promotion(string Title, string Description, string Code);
