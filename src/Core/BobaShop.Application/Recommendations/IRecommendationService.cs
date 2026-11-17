using BobaShop.Application.Recommendations.Dtos;

namespace BobaShop.Application.Recommendations;

public interface IRecommendationService
{
    Task<IReadOnlyList<RecommendedProductDto>> GetPopularAsync(
        int? days = null, int? topN = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendedProductDto>> GetTopRatedAsync(
        int? topN = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendedProductDto>> GetNewArrivalsAsync(
        int? topN = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecommendedProductDto>> GetForUserAsync(
        int userId, int? topN = null, CancellationToken cancellationToken = default);
}
