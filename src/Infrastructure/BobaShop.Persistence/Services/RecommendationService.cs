using BobaShop.Application.Recommendations;
using BobaShop.Application.Recommendations.Dtos;
using BobaShop.Domain.Enums;
using BobaShop.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BobaShop.Persistence.Services;

public sealed class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _db;
    private readonly RecommendationOptions _options;

    public RecommendationService(ApplicationDbContext db, RecommendationOptions options)
    {
        _db = db;
        _options = options;
        options.Validate();
    }

    public async Task<IReadOnlyList<RecommendedProductDto>> GetPopularAsync(
        int? days = null, int? topN = null, CancellationToken cancellationToken = default)
    {
        var limit = GetLimit(topN);
        var window = days ?? _options.PopularDays;
        if (window is < 1 or > RecommendationOptions.MaxPopularDays)
            throw new ArgumentOutOfRangeException(nameof(days));
        var now = DateTime.UtcNow;
        var since = now.AddDays(-window);
        var reason = window == 7 ? "Bán chạy tuần này" : $"Bán chạy {window} ngày qua";
        return await _db.OrderDetails.AsNoTracking()
            .Where(d => d.Product.IsAvailable && d.Order.Status == OrderStatus.Completed &&
                d.Order.CreatedAt >= since && d.Order.CreatedAt <= now)
            .GroupBy(d => new { d.ProductID, d.Product.ProductName, d.Product.Price, d.Product.ImageURL })
            .Select(g => new { g.Key, Quantity = g.Sum(d => (long)d.Quantity) })
            .OrderByDescending(x => x.Quantity).ThenBy(x => x.Key.ProductID).Take(limit)
            .Select(x => new RecommendedProductDto
            {
                ProductID = x.Key.ProductID, Name = x.Key.ProductName, Price = x.Key.Price,
                ImageURL = x.Key.ImageURL, Score = x.Quantity, Reason = reason
            }).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecommendedProductDto>> GetTopRatedAsync(
        int? topN = null, CancellationToken cancellationToken = default)
    {
        var limit = GetLimit(topN);
        var now = DateTime.UtcNow;
        return await _db.Feedbacks.AsNoTracking()
            .Where(f => f.Product.IsAvailable && f.Rating >= 1 && f.Rating <= 5 &&
                f.CreatedAt <= now && f.Order.CreatedAt <= now && f.Order.Status == OrderStatus.Completed &&
                f.Order.UserID == f.UserID && f.Order.OrderDetails.Any(d => d.ProductID == f.ProductID))
            .GroupBy(f => new { f.ProductID, f.Product.ProductName, f.Product.Price, f.Product.ImageURL })
            .Select(g => new { g.Key, Reviews = g.Count(), Average = g.Average(f => (double)f.Rating) })
            .Where(x => x.Reviews >= _options.MinReviews)
            .OrderByDescending(x => x.Average).ThenByDescending(x => x.Reviews).ThenBy(x => x.Key.ProductID)
            .Take(limit).Select(x => new RecommendedProductDto
            {
                ProductID = x.Key.ProductID, Name = x.Key.ProductName, Price = x.Key.Price,
                ImageURL = x.Key.ImageURL, Score = x.Average, Reason = "Được đánh giá cao"
            }).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecommendedProductDto>> GetNewArrivalsAsync(
        int? topN = null, CancellationToken cancellationToken = default)
    {
        var limit = GetLimit(topN);
        var now = DateTime.UtcNow;
        var since = now.AddDays(-_options.NewArrivalDays);
        return await _db.Products.AsNoTracking()
            .Where(p => p.IsAvailable && p.CreatedAt >= since && p.CreatedAt <= now)
            .OrderByDescending(p => p.CreatedAt).ThenBy(p => p.ProductID).Take(limit)
            .Select(p => new RecommendedProductDto
            {
                ProductID = p.ProductID, Name = p.ProductName, Price = p.Price,
                ImageURL = p.ImageURL, Score = 0, Reason = "Món mới"
            }).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecommendedProductDto>> GetForUserAsync(
        int userId, int? topN = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        var limit = GetLimit(topN);
        var now = DateTime.UtcNow;
        var orders = _db.Orders.AsNoTracking().Where(o => o.UserID == userId &&
            o.Status == OrderStatus.Completed && o.CreatedAt <= now);
        if (await orders.CountAsync(cancellationToken) < _options.MinOrdersForPersonal)
            return await GetFallbackAsync(limit, cancellationToken);

        // Aggregate in SQL; only this user's product/configuration totals enter memory.
        var history = await _db.OrderDetails.AsNoTracking()
            .Where(d => d.Order.UserID == userId && d.Order.Status == OrderStatus.Completed && d.Order.CreatedAt <= now)
            .GroupBy(d => new
            {
                d.ProductID, d.Product.CategoryID, d.Product.ProductName, d.Product.Price,
                d.Product.ImageURL, d.Product.IsAvailable, d.Size, d.SugarLevel, d.IceLevel
            })
            .Select(g => new HistoryRow(g.Key.ProductID, g.Key.CategoryID, g.Key.ProductName,
                g.Key.Price, g.Key.ImageURL, g.Key.IsAvailable, g.Key.Size, g.Key.SugarLevel,
                g.Key.IceLevel, g.Sum(d => (long)d.Quantity))).ToListAsync(cancellationToken);
        if (history.Count == 0) return await GetFallbackAsync(limit, cancellationToken);

        var categoryWeights = history.GroupBy(h => h.CategoryID)
            .ToDictionary(g => g.Key, g => g.Sum(h => h.Quantity));
        var purchasedIds = history.Select(h => h.ProductID).Distinct().ToArray();
        var categoryIds = categoryWeights.Keys.ToArray();
        var repeat = history.Where(h => h.IsAvailable).GroupBy(h => h.ProductID)
            .OrderByDescending(g => g.Sum(h => h.Quantity)).ThenBy(g => g.Key).Take(limit)
            .Select(g => CreatePersonal(g.First(), g, g.Sum(h => h.Quantity), "Bạn hay đặt")).ToList();
        var candidates = await _db.Products.AsNoTracking()
            .Where(p => p.IsAvailable && categoryIds.Contains(p.CategoryID) && !purchasedIds.Contains(p.ProductID))
            .Select(p => new { p.ProductID, p.CategoryID, p.ProductName, p.Price, p.ImageURL })
            .ToListAsync(cancellationToken);
        var discovery = candidates.OrderByDescending(p => categoryWeights[p.CategoryID])
            .ThenBy(p => p.ProductID).Take(limit)
            .Select(p => CreatePersonal(new HistoryRow(p.ProductID, p.CategoryID, p.ProductName, p.Price,
                p.ImageURL, true, default, default, default, 0),
                history.Where(h => h.CategoryID == p.CategoryID), categoryWeights[p.CategoryID],
                "Cùng loại với món bạn thích")).ToList();

        // Alternate familiar and unseen products so both personal signals remain visible.
        var result = new List<RecommendedProductDto>(limit);
        for (var i = 0; result.Count < limit && (i < repeat.Count || i < discovery.Count); i++)
        {
            if (i < repeat.Count) result.Add(repeat[i]);
            if (result.Count < limit && i < discovery.Count) result.Add(discovery[i]);
        }
        return result.Count == 0 ? await GetFallbackAsync(limit, cancellationToken) : result;
    }

    private async Task<IReadOnlyList<RecommendedProductDto>> GetFallbackAsync(int limit, CancellationToken cancellationToken)
    {
        var result = (await GetPopularAsync(topN: limit, cancellationToken: cancellationToken)).ToList();
        if (result.Count < limit) result.AddRange(await GetTopRatedAsync(limit, cancellationToken));
        result = result.DistinctBy(p => p.ProductID).Take(limit).ToList();
        if (result.Count < limit) result.AddRange(await GetNewArrivalsAsync(limit, cancellationToken));
        return result.DistinctBy(p => p.ProductID).Take(limit).ToList();
    }

    private int GetLimit(int? topN)
    {
        var limit = topN ?? _options.TopN;
        if (limit is < 1 or > RecommendationOptions.MaxTopN) throw new ArgumentOutOfRangeException(nameof(topN));
        return limit;
    }

    private static RecommendedProductDto CreatePersonal(HistoryRow product, IEnumerable<HistoryRow> history,
        double score, string reason)
    {
        var rows = history.ToList();
        return new RecommendedProductDto
        {
            ProductID = product.ProductID, Name = product.Name, Price = product.Price,
            ImageURL = product.ImageURL, Score = score, Reason = reason,
            PreferredSize = rows.GroupBy(h => h.Size).OrderByDescending(g => g.Sum(h => h.Quantity))
                .ThenBy(g => g.Key).First().Key,
            PreferredSugarLevel = rows.GroupBy(h => h.SugarLevel).OrderByDescending(g => g.Sum(h => h.Quantity))
                .ThenBy(g => g.Key).First().Key,
            PreferredIceLevel = rows.GroupBy(h => h.IceLevel).OrderByDescending(g => g.Sum(h => h.Quantity))
                .ThenBy(g => g.Key).First().Key
        };
    }

    private sealed record HistoryRow(int ProductID, int CategoryID, string Name, decimal Price,
        string? ImageURL, bool IsAvailable, DrinkSize Size, SugarLevel SugarLevel, IceLevel IceLevel, long Quantity);
}
