namespace BobaShop.Application.Recommendations;

public sealed class RecommendationOptions
{
    public const string SectionName = "Recommendations";
    public const int MaxTopN = 50;
    public const int MaxPopularDays = 365;

    public int TopN { get; set; } = 8;
    public int PopularDays { get; set; } = 7;
    public int NewArrivalDays { get; set; } = 14;
    public int MinReviews { get; set; } = 5;
    public int MinOrdersForPersonal { get; set; } = 3;

    public void Validate()
    {
        if (TopN is < 1 or > MaxTopN)
            throw new ArgumentOutOfRangeException(nameof(TopN), $"TopN must be between 1 and {MaxTopN}.");
        if (PopularDays is < 1 or > MaxPopularDays)
            throw new ArgumentOutOfRangeException(nameof(PopularDays), $"PopularDays must be between 1 and {MaxPopularDays}.");
        if (NewArrivalDays is < 1 or > MaxPopularDays)
            throw new ArgumentOutOfRangeException(nameof(NewArrivalDays), $"NewArrivalDays must be between 1 and {MaxPopularDays}.");
        if (MinReviews < 1)
            throw new ArgumentOutOfRangeException(nameof(MinReviews), "MinReviews must be positive.");
        if (MinOrdersForPersonal < 1)
            throw new ArgumentOutOfRangeException(nameof(MinOrdersForPersonal), "MinOrdersForPersonal must be positive.");
    }
}
