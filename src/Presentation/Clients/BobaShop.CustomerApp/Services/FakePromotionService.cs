namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Bản giả để dựng khu vực khuyến mãi khi API chưa có (theo mục 4 của Quy chuẩn).
/// Nội dung khuyến mãi dưới đây chỉ là dữ liệu mẫu để xem giao diện.
/// </summary>
public sealed class FakePromotionService : IPromotionService
{
    private static readonly TimeSpan FakeNetworkDelay = TimeSpan.FromMilliseconds(400);

    private static readonly IReadOnlyList<Promotion> Promotions = new[]
    {
        new Promotion("Giảm 20% đơn đầu tiên", "Dành cho tài khoản mới, áp dụng cho đơn từ 50.000 đ.", "BOBAMOI"),
        new Promotion("Giảm 10.000 đ đơn từ 80.000 đ", "Áp dụng mọi món trong thực đơn, mỗi tài khoản dùng một lần.", "GIAM10K"),
        new Promotion("Miễn phí một phần topping", "Tặng một topping bất kỳ khi mua từ hai ly trở lên.", "TOPPING0D"),
    };

    public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(FakeNetworkDelay, cancellationToken);
        return Promotions;
    }
}
