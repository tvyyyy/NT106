using System.Security.Claims;
using BobaShop.Application.Common.Constants;
using BobaShop.Application.Contracts;
using BobaShop.Application.Recommendations;
using BobaShop.Application.Recommendations.Dtos;

namespace BobaShop.WebAPI.Endpoints;

public static class RecommendationEndpoints
{
    public static IEndpointRouteBuilder MapRecommendationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.Recommendations.Base).WithTags("Recommendations");

        group.MapGet(ApiRoutes.Recommendations.Popular, async (
            int? days, int? topN, IRecommendationService service, CancellationToken ct) =>
        {
            if (!ValidTopN(topN) || days is < 1 or > RecommendationOptions.MaxPopularDays)
                return InvalidParameters();
            return Success(await service.GetPopularAsync(days, topN, ct));
        });

        group.MapGet(ApiRoutes.Recommendations.TopRated, async (
            int? topN, IRecommendationService service, CancellationToken ct) =>
            !ValidTopN(topN) ? InvalidParameters() : Success(await service.GetTopRatedAsync(topN, ct)));

        group.MapGet(ApiRoutes.Recommendations.New, async (
            int? topN, IRecommendationService service, CancellationToken ct) =>
            !ValidTopN(topN) ? InvalidParameters() : Success(await service.GetNewArrivalsAsync(topN, ct)));

        group.MapGet(ApiRoutes.Recommendations.ForYou, async (
            int? userId, int? topN, ClaimsPrincipal user, IRecommendationService service, CancellationToken ct) =>
        {
            if (!ValidTopN(topN) || userId is < 1)
                return InvalidParameters();
            if (!int.TryParse(user.FindFirstValue("sub"), out var currentUserId) || currentUserId < 1)
                return Results.Unauthorized();
            if (userId.HasValue && userId.Value != currentUserId)
                return Results.Forbid();
            return Success(await service.GetForUserAsync(currentUserId, topN, ct));
        }).RequireAuthorization();

        return endpoints;
    }

    private static bool ValidTopN(int? topN) => topN is null or >= 1 and <= RecommendationOptions.MaxTopN;

    private static IResult InvalidParameters() => Results.BadRequest(ApiResponse<object>.Fail(
        $"topN phải từ 1 đến {RecommendationOptions.MaxTopN}, days từ 1 đến {RecommendationOptions.MaxPopularDays}, userId phải dương."));

    private static IResult Success(IReadOnlyList<RecommendedProductDto> data) =>
        Results.Ok(ApiResponse<IReadOnlyList<RecommendedProductDto>>.Ok("Lấy gợi ý món thành công.", data));
}
