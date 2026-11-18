using BobaShop.Domain.Enums;

namespace BobaShop.Persistence.Seeding;

/// <summary>Một giá trị kèm trọng số, dùng cho các lựa chọn ngẫu nhiên có xác suất.</summary>
internal sealed record Weighted<T>(T Value, double Weight);

/// <summary>
/// Một nhóm sở thích của khách. Đây là "cấu trúc ẩn" của dữ liệu seed: khách cùng nhóm có
/// xác suất danh mục, size, đường, đá và topping giống nhau, để Cấp 2 (và sau này Cấp 3) có tín hiệu thật.
/// </summary>
/// <param name="CategoryWeights">Xác suất chọn từng danh mục. Rỗng nghĩa là nhóm không có đơn.</param>
/// <param name="Sizes">Phân phối size. Null nghĩa là chọn ngẫu nhiên đều.</param>
/// <param name="FavoriteToppings">Mã topping hay chọn. Rỗng nghĩa là chọn ngẫu nhiên theo trọng số catalog.</param>
internal sealed record UserProfile(
    string Code,
    string Name,
    int UserCount,
    int MinOrders,
    int MaxOrders,
    IReadOnlyList<Weighted<string>> CategoryWeights,
    IReadOnlyList<Weighted<DrinkSize>>? Sizes,
    IReadOnlyList<Weighted<SugarLevel>>? SugarLevels,
    IReadOnlyList<Weighted<IceLevel>>? IceLevels,
    double ToppingProbability,
    IReadOnlyList<string> FavoriteToppings);

/// <summary>Luật topping: món thỏa điều kiện thì thêm một trong các topping, với xác suất cho trước.</summary>
/// <param name="ProbabilityOverrides">Xác suất riêng cho từng nhóm (theo mã nhóm), nếu có.</param>
internal sealed record ToppingRule(
    string Description,
    Func<CatalogProduct, bool> Applies,
    IReadOnlyList<string> ToppingCodes,
    double Probability,
    IReadOnlyDictionary<string, double>? ProbabilityOverrides = null)
{
    public double ProbabilityFor(string profileCode)
    {
        return ProbabilityOverrides is not null && ProbabilityOverrides.TryGetValue(profileCode, out var value)
            ? value
            : Probability;
    }
}

/// <summary>Luật cặp món: đơn có món TriggerCode thì thêm món PairCode vào cùng đơn, với xác suất cho trước.</summary>
internal sealed record PairRule(string TriggerCode, string PairCode, double Probability);

/// <summary>Phân phối số sao và tỷ lệ có feedback riêng cho một món, để tạo sẵn ca kiểm thử.</summary>
internal sealed record RatingOverride(string ProductCode, double FeedbackRate, IReadOnlyList<Weighted<int>> Ratings);

/// <summary>
/// Nhóm user, xác suất và luật seed, chuyển từ sheet "Nhóm user &amp; luật seed" của phân công v3.
/// Mọi con số ở đây là tham số chỉnh được.
/// </summary>
internal static class SeedProfiles
{
    // Bảng 3: tham số chung.
    public const int RandomSeed = 42;
    public const int OrderHistoryDays = 75;
    public const int UserCreatedMaxDaysAgo = 90;

    /// <summary>Nhóm có số đơn tối thiểu không vượt mức này là nhóm "ít lịch sử" (Cấp 2 phải lùi về Cấp 1).</summary>
    public const int LowHistoryMaxOrders = 2;
    public const double FeedbackRate = 0.25;
    public const string EmailDomain = "@seed.local";
    public const string PhonePrefix = "0900";

    /// <summary>Món có xu hướng tăng: trọng số nhân thêm trong những ngày gần nhất.</summary>
    public static readonly IReadOnlySet<string> TrendingCodes = new HashSet<string> { "TS07", "TT01" };
    public const double TrendMultiplier = 1.5;
    public const int TrendDays = 7;

    /// <summary>Món mới chỉ có đúng số feedback 5 sao này, để kiểm thử ngưỡng MinReviews.</summary>
    public const string LowReviewNewProductCode = "TT06";
    public const int LowReviewNewProductFeedbackCount = 2;

    public static readonly IReadOnlyList<Weighted<OrderStatus>> OrderStatuses = new Weighted<OrderStatus>[]
    {
        new(OrderStatus.Completed, 0.88),
        new(OrderStatus.Cancelled, 0.08),
        new(OrderStatus.Pending, 0.02),
        new(OrderStatus.Preparing, 0.02),
    };

    /// <summary>Số dòng OrderDetails mỗi đơn (chưa tính món thêm theo luật cặp món).</summary>
    public static readonly IReadOnlyList<Weighted<int>> LinesPerOrder = new Weighted<int>[]
    {
        new(1, 0.70), new(2, 0.25), new(3, 0.05),
    };

    public static readonly IReadOnlyList<Weighted<int>> QuantityPerLine = new Weighted<int>[]
    {
        new(1, 0.90), new(2, 0.10),
    };

    public static readonly IReadOnlyList<Weighted<PaymentMethod>> PaymentMethods = new Weighted<PaymentMethod>[]
    {
        new(PaymentMethod.Cash, 0.6), new(PaymentMethod.PayOS, 0.4),
    };

    /// <summary>Tỷ lệ đơn tại chỗ, chỉ áp dụng khi CSDL đã có bàn.</summary>
    public const double DineInRate = 0.35;

    /// <summary>Xác suất một dòng có topping thì có thêm topping thứ hai.</summary>
    public const double SecondToppingRate = 0.25;

    /// <summary>Giờ đặt theo giờ Việt Nam, lệch về giờ cao điểm.</summary>
    public static readonly IReadOnlyList<Weighted<int>> LocalOrderHours = new Weighted<int>[]
    {
        new(7, 3), new(8, 5), new(9, 4), new(10, 3), new(11, 5), new(12, 6), new(13, 4),
        new(14, 3), new(15, 4), new(16, 5), new(17, 5), new(18, 4), new(19, 6), new(20, 5), new(21, 3),
    };

    /// <summary>Bảng 3, dòng 8: phân phối số sao mặc định, lệch về 4-5 sao.</summary>
    public static readonly IReadOnlyList<Weighted<int>> DefaultRatings = new Weighted<int>[]
    {
        new(5, 0.45), new(4, 0.35), new(3, 0.12), new(2, 0.05), new(1, 0.03),
    };

    /// <summary>Ca kiểm thử feedback: TS03 nhiều lượt trung bình ~4.3, TS06 trung bình ~3.2.</summary>
    public static readonly IReadOnlyList<RatingOverride> RatingOverrides = new RatingOverride[]
    {
        new("TS03", 0.60, new Weighted<int>[] { new(5, 0.45), new(4, 0.45), new(3, 0.10) }),
        new("TS06", 0.50, new Weighted<int>[] { new(5, 0.12), new(4, 0.28), new(3, 0.35), new(2, 0.15), new(1, 0.10) }),
    };

    // Bảng 1: nhóm người dùng. Thứ tự nhóm quyết định số thứ tự email:
    // A = user01-09, B = user10-16, C = user17-23, D = user24-28, E = user29-35, F = user36-43, G = user44-50.
    public static readonly IReadOnlyList<UserProfile> Profiles = new UserProfile[]
    {
        new(
            "A", "Mê trà sữa béo", 9, 15, 30,
            Categories(0.60, 0.05, 0.05, 0.15, 0.05, 0.10),
            new Weighted<DrinkSize>[] { new(DrinkSize.L, 0.85), new(DrinkSize.M, 0.15) },
            new Weighted<SugarLevel>[] { new(SugarLevel.Percent50, 0.6), new(SugarLevel.Percent70, 0.4) },
            new Weighted<IceLevel>[] { new(IceLevel.Normal, 0.85), new(IceLevel.Less, 0.10), new(IceLevel.Extra, 0.05) },
            0.8,
            new[] { "TP01", "TP05" }),
        new(
            "B", "Thích trà trái cây, ít đường", 7, 10, 20,
            Categories(0.05, 0.50, 0.03, 0.02, 0.20, 0.20),
            new Weighted<DrinkSize>[] { new(DrinkSize.M, 0.85), new(DrinkSize.S, 0.10), new(DrinkSize.L, 0.05) },
            new Weighted<SugarLevel>[] { new(SugarLevel.Percent30, 0.85), new(SugarLevel.Percent0, 0.10), new(SugarLevel.Percent50, 0.05) },
            new Weighted<IceLevel>[] { new(IceLevel.Less, 0.85), new(IceLevel.NoIce, 0.10), new(IceLevel.Normal, 0.05) },
            0.3,
            new[] { "TP03", "TP04" }),
        new(
            "C", "Nghiện cà phê", 7, 10, 20,
            Categories(0.08, 0.05, 0.70, 0.07, 0.10, 0.00),
            new Weighted<DrinkSize>[] { new(DrinkSize.M, 0.85), new(DrinkSize.S, 0.10), new(DrinkSize.L, 0.05) },
            new Weighted<SugarLevel>[] { new(SugarLevel.Percent0, 0.5), new(SugarLevel.Percent30, 0.5) },
            new Weighted<IceLevel>[] { new(IceLevel.Normal, 0.85), new(IceLevel.Less, 0.15) },
            0.15,
            new[] { "TP06", "TP12" }),
        new(
            "D", "Thích ngọt, đá xay", 5, 8, 16,
            Categories(0.25, 0.10, 0.05, 0.50, 0.00, 0.10),
            new Weighted<DrinkSize>[] { new(DrinkSize.L, 0.85), new(DrinkSize.M, 0.15) },
            new Weighted<SugarLevel>[] { new(SugarLevel.Percent100, 0.85), new(SugarLevel.Percent70, 0.15) },
            new Weighted<IceLevel>[] { new(IceLevel.Extra, 0.85), new(IceLevel.Normal, 0.15) },
            0.6,
            new[] { "TP07", "TP01" }),
        new(
            "E", "Thử đủ loại", 7, 8, 20,
            Categories(0.20, 0.20, 0.15, 0.15, 0.15, 0.15),
            null, null, null,
            0.4,
            Array.Empty<string>()),
        new(
            "F", "Ít lịch sử (1-2 đơn)", 8, 1, 2,
            Categories(0.17, 0.17, 0.17, 0.17, 0.16, 0.16),
            null, null, null,
            0.3,
            Array.Empty<string>()),
        new(
            "G", "Chưa có đơn nào", 7, 0, 0,
            Array.Empty<Weighted<string>>(),
            null, null, null,
            0,
            Array.Empty<string>()),
    };

    // Bảng 2: luật topping.
    public static readonly IReadOnlyList<ToppingRule> ToppingRules = new ToppingRule[]
    {
        new(
            "Trà sữa thêm trân châu đen",
            product => product.Category == SeedCatalog.MilkTea,
            new[] { "TP01" },
            0.6,
            new Dictionary<string, double> { ["A"] = 0.8 }),
        new(
            "Trà sữa matcha hoặc cà phê muối thêm kem cheese",
            product => product.Code is "TS04" or "CF04",
            new[] { "TP06" },
            0.35),
        new(
            "Trà trái cây thêm thạch dừa hoặc thạch trái cây",
            product => product.Category == SeedCatalog.FruitTea,
            new[] { "TP03", "TP04" },
            0.4),
        new(
            "Đá xay thêm kem sữa",
            product => product.Category == SeedCatalog.IceBlended,
            new[] { "TP07" },
            0.5),
    };

    // Bảng 2: luật cặp món đi cùng.
    public static readonly IReadOnlyList<PairRule> PairRules = new PairRule[]
    {
        new("TS01", "SC03", 0.20),
        new("TT01", "TT04", 0.15),
        new("TS04", "DX01", 0.25),
        new("CF01", "CF03", 0.15),
        new("SC01", "TT01", 0.25),
    };

    private static Weighted<string>[] Categories(
        double milkTea,
        double fruitTea,
        double coffee,
        double iceBlended,
        double pureTea,
        double yogurt)
    {
        return new Weighted<string>[]
        {
            new(SeedCatalog.MilkTea, milkTea),
            new(SeedCatalog.FruitTea, fruitTea),
            new(SeedCatalog.Coffee, coffee),
            new(SeedCatalog.IceBlended, iceBlended),
            new(SeedCatalog.PureTea, pureTea),
            new(SeedCatalog.Yogurt, yogurt),
        };
    }
}
