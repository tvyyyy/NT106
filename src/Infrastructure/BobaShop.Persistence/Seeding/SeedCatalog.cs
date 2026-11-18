namespace BobaShop.Persistence.Seeding;

/// <summary>Một món trong catalog mẫu. Price là giá size S.</summary>
/// <param name="Code">Mã nội bộ của seeder (TS01, CF03...), không lưu vào CSDL.</param>
/// <param name="Popularity">Trọng số phổ biến 1-10, dùng khi chọn món cho đơn.</param>
/// <param name="AddedDaysAgo">Số ngày trước hôm nay món được thêm vào thực đơn (Products.CreatedAt).</param>
internal sealed record CatalogProduct(
    string Code,
    string Name,
    string Category,
    decimal Price,
    int Popularity,
    bool IsAvailable,
    int AddedDaysAgo);

/// <summary>Một topping trong catalog mẫu.</summary>
/// <param name="Weight">Trọng số chọn 1-10, dùng khi chọn topping ngẫu nhiên.</param>
internal sealed record CatalogTopping(string Code, string Name, decimal Price, int Weight, bool IsAvailable);

/// <summary>
/// Catalog mẫu cho hệ khuyến nghị, chuyển từ sheet "Catalog - Món" và "Catalog - Topping" của phân công v3:
/// 6 danh mục, 35 món (4 món mới trong 14 ngày, 2 món hết hàng) và 12 topping.
/// Giá và phụ thu size là giả định của nhóm, chưa phải menu thật.
/// </summary>
internal static class SeedCatalog
{
    public const string MilkTea = "Trà sữa";
    public const string FruitTea = "Trà trái cây";
    public const string Coffee = "Cà phê";
    public const string IceBlended = "Đá xay";
    public const string PureTea = "Trà nguyên bản";
    public const string Yogurt = "Sữa chua & đặc biệt";

    /// <summary>Phụ thu so với giá size S. UnitPrice = Product.Price + phụ thu size.</summary>
    public const decimal SizeMSurcharge = 5000m;
    public const decimal SizeLSurcharge = 10000m;

    /// <summary>Món hết hàng chỉ xuất hiện trong đơn cũ hơn số ngày này (hết hàng gần đây).</summary>
    public const int UnavailableSinceDaysAgo = 14;

    public static readonly IReadOnlyList<string> Categories = new[]
    {
        MilkTea, FruitTea, Coffee, IceBlended, PureTea, Yogurt,
    };

    public static readonly IReadOnlyList<CatalogProduct> Products = new CatalogProduct[]
    {
        new("TS01", "Trà sữa trân châu đường đen", MilkTea, 35000m, 10, true, 90),
        new("TS02", "Trà sữa truyền thống", MilkTea, 30000m, 8, true, 90),
        new("TS03", "Hồng trà sữa", MilkTea, 32000m, 6, true, 88),
        new("TS04", "Trà sữa matcha", MilkTea, 38000m, 7, true, 86),
        new("TS05", "Trà sữa khoai môn", MilkTea, 36000m, 5, true, 84),
        new("TS06", "Trà sữa Thái xanh", MilkTea, 34000m, 4, true, 82),
        new("TS07", "Trà sữa oolong nướng", MilkTea, 38000m, 6, true, 70),
        new("TS08", "Trà sữa socola", MilkTea, 36000m, 3, false, 80),

        new("TT01", "Trà đào cam sả", FruitTea, 38000m, 9, true, 90),
        new("TT02", "Trà vải hoa hồng", FruitTea, 36000m, 5, true, 87),
        new("TT03", "Trà dâu tằm", FruitTea, 36000m, 4, true, 75),
        new("TT04", "Trà chanh dây", FruitTea, 34000m, 5, true, 89),
        new("TT05", "Trà xoài nhiệt đới", FruitTea, 38000m, 3, true, 65),
        new("TT06", "Trà ổi hồng", FruitTea, 38000m, 2, true, 10),

        new("CF01", "Cà phê sữa đá", Coffee, 29000m, 8, true, 90),
        new("CF02", "Cà phê đen đá", Coffee, 25000m, 5, true, 90),
        new("CF03", "Bạc xỉu", Coffee, 32000m, 6, true, 85),
        new("CF04", "Cà phê muối", Coffee, 35000m, 6, true, 78),
        new("CF05", "Cappuccino", Coffee, 42000m, 3, true, 72),
        new("CF06", "Cold brew cam", Coffee, 45000m, 2, true, 6),

        new("DX01", "Matcha đá xay", IceBlended, 45000m, 6, true, 88),
        new("DX02", "Socola đá xay", IceBlended, 45000m, 5, true, 83),
        new("DX03", "Cookie & Cream đá xay", IceBlended, 48000m, 5, true, 80),
        new("DX04", "Xoài đá xay", IceBlended, 42000m, 4, true, 68),
        new("DX05", "Dâu đá xay", IceBlended, 42000m, 3, true, 12),
        new("DX06", "Cà phê đá xay", IceBlended, 42000m, 4, true, 76),

        new("TN01", "Trà đen", PureTea, 22000m, 3, true, 90),
        new("TN02", "Trà xanh nhài", PureTea, 22000m, 3, true, 90),
        new("TN03", "Oolong", PureTea, 25000m, 2, true, 74),
        new("TN04", "Trà gạo lứt", PureTea, 25000m, 2, false, 81),

        new("SC01", "Sữa chua trân châu", Yogurt, 35000m, 5, true, 86),
        new("SC02", "Sữa chua dâu", Yogurt, 38000m, 3, true, 79),
        new("SC03", "Sữa tươi trân châu đường đen", Yogurt, 38000m, 7, true, 90),
        new("SC04", "Yakult đào", Yogurt, 38000m, 3, true, 4),
        new("SC05", "Sữa chua xoài", Yogurt, 38000m, 2, true, 62),
    };

    public static readonly IReadOnlyList<CatalogTopping> Toppings = new CatalogTopping[]
    {
        new("TP01", "Trân châu đen", 8000m, 10, true),
        new("TP02", "Trân châu trắng", 8000m, 4, true),
        new("TP03", "Thạch dừa", 7000m, 6, true),
        new("TP04", "Thạch trái cây", 7000m, 5, true),
        new("TP05", "Pudding trứng", 10000m, 7, true),
        new("TP06", "Kem cheese", 12000m, 6, true),
        new("TP07", "Kem sữa (whipping)", 10000m, 5, true),
        new("TP08", "Sương sáo", 7000m, 3, true),
        new("TP09", "Trân châu hoàng kim", 10000m, 4, true),
        new("TP10", "Thạch matcha", 8000m, 3, true),
        new("TP11", "Hạt đác", 8000m, 2, true),
        new("TP12", "Bánh flan", 12000m, 4, true),
    };
}
