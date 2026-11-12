using BobaShop.ClientNetworkLogic;
using HomeDtos = BobaShop.Application.Home;

namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Service khuyến mãi gọi GET /api/promotions (banner ưu đãi tạo từ voucher đang hoạt động).
/// Lỗi thì ném InvalidOperationException kèm thông báo, trang chủ tự bắt và báo trên giao diện.
/// </summary>
public sealed class PromotionService : IPromotionService
{
    private readonly ApiClient _apiClient;

    public PromotionService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.GetAsync<List<HomeDtos.PromotionDto>>("api/promotions", ct: cancellationToken);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message);
        }

        return (result.Data ?? new List<HomeDtos.PromotionDto>())
            .Select(promotion => new Promotion(promotion.Title, promotion.Description, promotion.Code))
            .ToList();
    }
}
