namespace BobaShop.CustomerApp.Services;

/// <summary>Service khuyến mãi phía client. Form chỉ gọi interface này, không gọi ApiClient hay HttpClient.</summary>
public interface IPromotionService
{
    /// <summary>Các khuyến mãi đang chạy, hiển thị ở trang chủ.</summary>
    Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default);
}
