using BobaShop.Domain.Enums;

namespace BobaShop.Application.Recommendations.Dtos;

public sealed class RecommendedProductDto
{
    public int ProductID { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? ImageURL { get; init; }
    public double Score { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DrinkSize? PreferredSize { get; init; }
    public SugarLevel? PreferredSugarLevel { get; init; }
    public IceLevel? PreferredIceLevel { get; init; }
}
