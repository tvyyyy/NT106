using BobaShop.Application.Auth;
using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using BobaShop.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BobaShop.Persistence.Seeding;

/// <summary>Số dòng seeder đã tạo hoặc đã xóa, để in ra khi chạy lệnh.</summary>
public sealed record SeedSummary(
    bool Skipped,
    int Users,
    int Orders,
    int OrderDetails,
    int OrderDetailToppings,
    int Feedbacks)
{
    public override string ToString()
    {
        return Skipped
            ? "Đã có dữ liệu seed (user @seed.local), bỏ qua. Chạy --clear-seed trước nếu muốn sinh lại."
            : $"Users: {Users}, Orders: {Orders}, OrderDetails: {OrderDetails}, "
              + $"OrderDetailToppings: {OrderDetailToppings}, Feedbacks: {Feedbacks}";
    }
}

/// <summary>
/// Sinh dữ liệu demo cho hệ khuyến nghị vào các bảng thật: catalog món/topping, 50 khách thuộc 7 nhóm sở thích,
/// đơn hàng 75 ngày gần nhất, chi tiết, topping và feedback. Dùng Random với seed cố định nên chạy lại cho cùng
/// cấu trúc dữ liệu (ngày giờ tính lại theo thời điểm chạy). Chỉ gọi ở môi trường Development.
/// Dữ liệu seed nhận diện bằng email @seed.local; ClearSeedAsync xóa đúng phần này, không đụng dữ liệu thật.
/// </summary>
public static class DataSeeder
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    private static readonly string[] LastNames =
    {
        "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương",
    };

    private static readonly string[] MiddleNames =
    {
        "Văn", "Thị", "Minh", "Ngọc", "Thanh", "Gia", "Hoài", "Quốc", "Bảo", "Khánh",
    };

    private static readonly string[] GivenNames =
    {
        "An", "Bình", "Chi", "Dũng", "Giang", "Hà", "Huy", "Khoa", "Lan", "Linh",
        "My", "Nam", "Phúc", "Quân", "Thảo", "Trang", "Tú", "Vy", "Yến", "Long",
    };

    private static readonly Dictionary<int, string[]> CommentsByRating = new()
    {
        [5] = new[] { "Rất ngon, sẽ quay lại", "Vị đậm, đúng gu", "Ngon xuất sắc", "Uống hoài không chán" },
        [4] = new[] { "Ngon, giao nhanh", "Khá ổn", "Vị vừa miệng", "Ổn, giá hợp lý" },
        [3] = new[] { "Bình thường", "Tạm được", "Hơi ngọt so với mình" },
        [2] = new[] { "Vị hơi nhạt", "Không như kỳ vọng", "Đá tan nhanh, mất vị" },
        [1] = new[] { "Không hợp khẩu vị", "Quá ngọt, không uống hết" },
    };

    /// <summary>Tỷ lệ feedback không kèm nhận xét.</summary>
    private const double CommentlessRate = 0.4;

    /// <summary>
    /// Sinh toàn bộ dữ liệu seed trong một transaction. Bỏ qua nếu đã có user @seed.local (idempotent).
    /// </summary>
    /// <param name="password">
    /// Mật khẩu chung của các tài khoản seed, để đăng nhập thử khối "Dành cho bạn". Lấy từ cấu hình
    /// (User Secrets), không ghi cứng trong code.
    /// </param>
    public static async Task<SeedSummary> SeedAsync(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        string password,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Mật khẩu tài khoản seed không được để trống.", nameof(password));
        }

        if (await db.Users.AnyAsync(u => u.Email.EndsWith(SeedProfiles.EmailDomain), ct))
        {
            return new SeedSummary(true, 0, 0, 0, 0, 0);
        }

        var random = new Random(SeedProfiles.RandomSeed);
        var now = DateTime.UtcNow;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var catalog = await EnsureCatalogAsync(db, now, ct);
        var users = await CreateUsersAsync(db, passwordHasher, password, random, now, ct);
        var tables = await db.DiningTables.Where(t => t.IsActive).OrderBy(t => t.TableID).ToListAsync(ct);

        var drafts = CreateOrderDrafts(users, catalog, random, now);
        EnsureLinesForProduct(drafts, catalog, SeedProfiles.LowReviewNewProductCode,
            SeedProfiles.LowReviewNewProductFeedbackCount, random, now);

        var orders = drafts.Select(draft => ToOrder(draft, catalog, tables, random, now)).ToList();
        db.Orders.AddRange(orders);

        var feedbacks = CreateFeedbacks(drafts, orders, random, now);
        db.Feedbacks.AddRange(feedbacks);

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new SeedSummary(
            false,
            users.Count,
            orders.Count,
            orders.Sum(o => o.OrderDetails.Count),
            orders.Sum(o => o.OrderDetails.Sum(d => d.OrderDetailToppings.Count)),
            feedbacks.Count);
    }

    /// <summary>
    /// Xóa dữ liệu seed theo đúng thứ tự khóa ngoại: Feedbacks, OrderDetailToppings, OrderDetails, Orders,
    /// rồi các bảng con khác của user, cuối cùng là Users @seed.local. Không xóa catalog món/topping.
    /// </summary>
    public static async Task<SeedSummary> ClearSeedAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        var seedUserIds = db.Users
            .Where(u => u.Email.EndsWith(SeedProfiles.EmailDomain))
            .Select(u => u.UserID);

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var feedbacks = await db.Feedbacks
            .Where(f => seedUserIds.Contains(f.UserID) || seedUserIds.Contains(f.Order.UserID))
            .ExecuteDeleteAsync(ct);
        var toppings = await db.OrderDetailToppings
            .Where(t => seedUserIds.Contains(t.OrderDetail.Order.UserID))
            .ExecuteDeleteAsync(ct);
        var details = await db.OrderDetails
            .Where(d => seedUserIds.Contains(d.Order.UserID))
            .ExecuteDeleteAsync(ct);
        await db.VoucherRedemptions
            .Where(r => seedUserIds.Contains(r.UserID) || seedUserIds.Contains(r.Order.UserID))
            .ExecuteDeleteAsync(ct);
        var orders = await db.Orders
            .Where(o => seedUserIds.Contains(o.UserID))
            .ExecuteDeleteAsync(ct);
        await db.PasswordResetTokens
            .Where(t => seedUserIds.Contains(t.UserID))
            .ExecuteDeleteAsync(ct);
        var users = await db.Users
            .Where(u => u.Email.EndsWith(SeedProfiles.EmailDomain))
            .ExecuteDeleteAsync(ct);

        await transaction.CommitAsync(ct);
        return new SeedSummary(false, users, orders, details, toppings, feedbacks);
    }

    /// <summary>
    /// Tạo danh mục, món, topping còn thiếu theo SeedCatalog (khớp theo tên, giữ nguyên dòng đã có),
    /// rồi đặt IsAvailable và ngày thêm món theo catalog.
    /// </summary>
    private static async Task<CatalogMap> EnsureCatalogAsync(ApplicationDbContext db, DateTime now, CancellationToken ct)
    {
        var categories = await db.Categories.ToDictionaryAsync(c => c.CategoryName, ct);
        foreach (var name in SeedCatalog.Categories.Where(name => !categories.ContainsKey(name)))
        {
            var category = new Category { CategoryName = name };
            db.Categories.Add(category);
            categories[name] = category;
        }

        var existingProducts = await db.Products.Include(p => p.Category).ToListAsync(ct);
        var products = new Dictionary<string, Product>();
        foreach (var item in SeedCatalog.Products)
        {
            var product = existingProducts.FirstOrDefault(p =>
                p.Category.CategoryName == item.Category && p.ProductName == item.Name);
            if (product is null)
            {
                product = new Product { Category = categories[item.Category], ProductName = item.Name, Price = item.Price };
                db.Products.Add(product);
            }

            products[item.Code] = product;
        }

        var existingToppings = await db.Toppings.ToDictionaryAsync(t => t.ToppingName, ct);
        var toppings = new Dictionary<string, Topping>();
        foreach (var item in SeedCatalog.Toppings)
        {
            if (!existingToppings.TryGetValue(item.Name, out var topping))
            {
                topping = new Topping { ToppingName = item.Name, Price = item.Price };
                db.Toppings.Add(topping);
            }

            toppings[item.Code] = topping;
        }

        await db.SaveChangesAsync(ct);

        // IsAvailable có HasDefaultValue(true): EF không gửi giá trị false khi INSERT (false là giá trị mặc định
        // của bool), nên phải đặt sau khi đã lưu, lúc đó EF sinh câu UPDATE.
        // Products.CreatedAt chỉ có sau migration AddProductCreatedAt (việc STT 1); chưa có cột thì bỏ qua.
        var hasProductCreatedAt = db.Model.FindEntityType(typeof(Product))?.FindProperty("CreatedAt") is not null;
        foreach (var item in SeedCatalog.Products)
        {
            var product = products[item.Code];
            product.IsAvailable = item.IsAvailable;
            if (hasProductCreatedAt)
            {
                db.Entry(product).Property("CreatedAt").CurrentValue = now.AddDays(-item.AddedDaysAgo);
            }
        }

        foreach (var item in SeedCatalog.Toppings)
        {
            toppings[item.Code].IsAvailable = item.IsAvailable;
        }

        await db.SaveChangesAsync(ct);
        return new CatalogMap(products, toppings);
    }

    private static async Task<List<SeedUser>> CreateUsersAsync(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        string password,
        Random random,
        DateTime now,
        CancellationToken ct)
    {
        // Băm một lần rồi dùng chung: BCrypt chậm, băm 50 lần mất vài giây mà không thêm giá trị gì.
        var passwordHash = passwordHasher.Hash(password);
        var users = new List<SeedUser>();
        var index = 0;

        foreach (var profile in SeedProfiles.Profiles)
        {
            for (var i = 0; i < profile.UserCount; i++)
            {
                index++;
                var hasOrders = profile.MaxOrders > 0;

                // Khách có nhiều đơn được tạo trước cửa sổ lịch sử đơn để đơn rải đủ 75 ngày;
                // nhóm ít lịch sử và nhóm chưa có đơn rải đều trong 90 ngày.
                var createdDaysAgo = hasOrders && profile.MinOrders > SeedProfiles.LowHistoryMaxOrders
                    ? random.Next(SeedProfiles.OrderHistoryDays, SeedProfiles.UserCreatedMaxDaysAgo + 1)
                    : random.NextDouble() * SeedProfiles.UserCreatedMaxDaysAgo;

                var user = new User
                {
                    RoleID = (int)UserRole.Customer,
                    FullName = $"{Pick(LastNames, random)} {Pick(MiddleNames, random)} {Pick(GivenNames, random)}",
                    Email = $"user{index:D2}{SeedProfiles.EmailDomain}",
                    Phone = $"{SeedProfiles.PhonePrefix}{index:D7}",
                    PasswordHash = passwordHash,
                    CreatedAt = now.AddDays(-createdDaysAgo),
                };

                users.Add(new SeedUser(user, profile));
            }
        }

        var phones = users.Select(u => u.Entity.Phone).ToList();
        var conflicts = await db.Users.Where(u => phones.Contains(u.Phone)).Select(u => u.Phone).ToListAsync(ct);
        if (conflicts.Count > 0)
        {
            throw new InvalidOperationException(
                $"Số điện thoại seed trùng với tài khoản đã có: {string.Join(", ", conflicts)}. Đổi SeedProfiles.PhonePrefix.");
        }

        db.Users.AddRange(users.Select(u => u.Entity));
        await db.SaveChangesAsync(ct);
        return users;
    }

    private static List<OrderDraft> CreateOrderDrafts(
        IReadOnlyList<SeedUser> users,
        CatalogMap catalog,
        Random random,
        DateTime now)
    {
        var drafts = new List<OrderDraft>();
        var historyStart = now.AddDays(-SeedProfiles.OrderHistoryDays);

        foreach (var (user, profile) in users)
        {
            if (profile.MaxOrders <= 0)
            {
                continue;
            }

            var orderCount = random.Next(profile.MinOrders, profile.MaxOrders + 1);
            var windowStart = user.CreatedAt > historyStart ? user.CreatedAt : historyStart;

            for (var i = 0; i < orderCount; i++)
            {
                var status = PickWeighted(SeedProfiles.OrderStatuses, random);

                // Đơn còn đang chờ hoặc đang pha phải là đơn vừa đặt, không thể nằm từ tuần trước.
                var createdAt = status is OrderStatus.Pending or OrderStatus.Preparing
                    ? Max(now.AddMinutes(-random.Next(5, 90)), windowStart)
                    : PickOrderTime(windowStart, now, random);

                var draft = new OrderDraft(user, profile, createdAt, status);
                var lineCount = PickWeighted(SeedProfiles.LinesPerOrder, random);
                for (var line = 0; line < lineCount; line++)
                {
                    var item = PickProduct(profile, catalog, createdAt, now, random);
                    if (item is null)
                    {
                        break;
                    }

                    var existing = draft.Lines.FirstOrDefault(l => l.Item.Code == item.Code);
                    if (existing is not null)
                    {
                        // Không để một món lặp hai dòng trong cùng đơn (Feedbacks unique theo OrderID + ProductID).
                        existing.Quantity++;
                        continue;
                    }

                    draft.Lines.Add(CreateLine(item, profile, catalog, random));
                }

                ApplyPairRules(draft, catalog, now, random);
                if (draft.Lines.Count > 0)
                {
                    drafts.Add(draft);
                }
            }
        }

        return drafts.OrderBy(d => d.CreatedAt).ToList();
    }

    /// <summary>Đơn có món kích hoạt thì thêm món đi cùng vào đơn (luật cặp món ở Bảng 2).</summary>
    private static void ApplyPairRules(OrderDraft draft, CatalogMap catalog, DateTime now, Random random)
    {
        var triggers = draft.Lines.Select(l => l.Item.Code).ToList();
        foreach (var rule in SeedProfiles.PairRules)
        {
            if (!triggers.Contains(rule.TriggerCode)
                || draft.Lines.Any(l => l.Item.Code == rule.PairCode)
                || random.NextDouble() >= rule.Probability)
            {
                continue;
            }

            var pair = catalog.Item(rule.PairCode);
            if (IsOrderable(pair, draft.CreatedAt, now))
            {
                draft.Lines.Add(CreateLine(pair, draft.Profile, catalog, random));
            }
        }
    }

    /// <summary>
    /// Bảo đảm có ít nhất số dòng đơn Completed chứa món cho trước, để tạo đủ feedback cho ca kiểm thử.
    /// Thiếu thì thêm món vào các đơn Completed gần nhất đặt sau ngày món ra mắt, ưu tiên khách nhóm trà trái cây.
    /// </summary>
    private static void EnsureLinesForProduct(
        List<OrderDraft> drafts,
        CatalogMap catalog,
        string productCode,
        int minimumLines,
        Random random,
        DateTime now)
    {
        var item = catalog.Item(productCode);
        var current = drafts.Count(d => d.Status == OrderStatus.Completed && d.Lines.Any(l => l.Item.Code == productCode));
        if (current >= minimumLines)
        {
            return;
        }

        var addedAt = now.AddDays(-item.AddedDaysAgo);
        var candidates = drafts
            .Where(d => d.Status == OrderStatus.Completed
                && d.CreatedAt >= addedAt
                && d.Lines.All(l => l.Item.Code != productCode))
            .OrderByDescending(d => d.Profile.Code == "B")
            .ThenByDescending(d => d.CreatedAt)
            .Take(minimumLines - current);

        foreach (var draft in candidates)
        {
            draft.Lines.Add(CreateLine(item, draft.Profile, catalog, random));
        }
    }

    private static OrderLine CreateLine(CatalogProduct item, UserProfile profile, CatalogMap catalog, Random random)
    {
        var line = new OrderLine(item, catalog.Product(item.Code))
        {
            Quantity = PickWeighted(SeedProfiles.QuantityPerLine, random),
            Size = profile.Sizes is null ? PickEnum<DrinkSize>(random) : PickWeighted(profile.Sizes, random),
            SugarLevel = profile.SugarLevels is null ? PickEnum<SugarLevel>(random) : PickWeighted(profile.SugarLevels, random),
            IceLevel = profile.IceLevels is null ? PickEnum<IceLevel>(random) : PickWeighted(profile.IceLevels, random),
        };

        if (random.NextDouble() >= profile.ToppingProbability)
        {
            return line;
        }

        var chosen = new List<string>();
        foreach (var rule in SeedProfiles.ToppingRules)
        {
            if (rule.Applies(item) && random.NextDouble() < rule.ProbabilityFor(profile.Code))
            {
                AddTopping(chosen, Pick(rule.ToppingCodes, random), catalog);
            }
        }

        if (chosen.Count == 0)
        {
            AddTopping(chosen, PickTopping(profile, chosen, random), catalog);
        }

        if (random.NextDouble() < SeedProfiles.SecondToppingRate)
        {
            AddTopping(chosen, PickTopping(profile, chosen, random), catalog);
        }

        line.ToppingCodes.AddRange(chosen);
        return line;
    }

    private static void AddTopping(List<string> chosen, string? code, CatalogMap catalog)
    {
        if (code is not null && !chosen.Contains(code) && catalog.ToppingItem(code).IsAvailable)
        {
            chosen.Add(code);
        }
    }

    private static string? PickTopping(UserProfile profile, List<string> exclude, Random random)
    {
        var favorites = profile.FavoriteToppings.Where(code => !exclude.Contains(code)).ToList();
        if (favorites.Count > 0)
        {
            return Pick(favorites, random);
        }

        var options = SeedCatalog.Toppings
            .Where(t => t.IsAvailable && !exclude.Contains(t.Code))
            .Select(t => new Weighted<string>(t.Code, t.Weight))
            .ToList();
        return options.Count == 0 ? null : PickWeighted(options, random);
    }

    /// <summary>
    /// Chọn danh mục theo xác suất của nhóm, rồi chọn món theo trọng số phổ biến (có nhân xu hướng tuần gần nhất).
    /// Chỉ chọn món đã ra mắt tại thời điểm đặt; món hết hàng chỉ xuất hiện trong đơn cũ.
    /// </summary>
    private static CatalogProduct? PickProduct(
        UserProfile profile,
        CatalogMap catalog,
        DateTime orderTime,
        DateTime now,
        Random random)
    {
        const int maxAttempts = 5;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var category = PickWeighted(profile.CategoryWeights, random);
            var options = SeedCatalog.Products
                .Where(p => p.Category == category && IsOrderable(p, orderTime, now))
                .Select(p => new Weighted<CatalogProduct>(p, ProductWeight(p, orderTime, now)))
                .ToList();
            if (options.Count > 0)
            {
                return PickWeighted(options, random);
            }
        }

        return null;
    }

    private static double ProductWeight(CatalogProduct item, DateTime orderTime, DateTime now)
    {
        var trending = SeedProfiles.TrendingCodes.Contains(item.Code)
            && orderTime >= now.AddDays(-SeedProfiles.TrendDays);
        return trending ? item.Popularity * SeedProfiles.TrendMultiplier : item.Popularity;
    }

    private static bool IsOrderable(CatalogProduct item, DateTime orderTime, DateTime now)
    {
        if (orderTime < now.AddDays(-item.AddedDaysAgo))
        {
            return false;
        }

        return item.IsAvailable || orderTime < now.AddDays(-SeedCatalog.UnavailableSinceDaysAgo);
    }

    /// <summary>Chuyển bản nháp thành entity, tính tiền theo đúng quy tắc của dự án.</summary>
    private static Order ToOrder(
        OrderDraft draft,
        CatalogMap catalog,
        IReadOnlyList<DiningTable> tables,
        Random random,
        DateTime now)
    {
        var details = draft.Lines.Select(line =>
        {
            var detail = new OrderDetail
            {
                Product = line.Product,
                Quantity = line.Quantity,
                Size = line.Size,
                SugarLevel = line.SugarLevel,
                IceLevel = line.IceLevel,
                UnitPrice = line.Product.Price + SizeSurcharge(line.Size),
            };

            foreach (var code in line.ToppingCodes)
            {
                var topping = catalog.Topping(code);
                detail.OrderDetailToppings.Add(new OrderDetailTopping { Topping = topping, Price = topping.Price });
            }

            return detail;
        }).ToList();

        // TODO: khi tầng Application có lớp tính tiền đơn dùng chung thì gọi lớp đó thay cho dòng dưới,
        // để công thức chỉ nằm một nơi.
        // Thành tiền dòng = (UnitPrice + tổng giá topping) × Quantity; TotalAmount = tổng các dòng.
        var totalAmount = details.Sum(d => (d.UnitPrice + d.OrderDetailToppings.Sum(t => t.Price)) * d.Quantity);

        var paymentMethod = PickWeighted(SeedProfiles.PaymentMethods, random);
        var isPaid = draft.Status == OrderStatus.Completed
            || (draft.Status == OrderStatus.Preparing && paymentMethod == PaymentMethod.PayOS);
        var dineIn = tables.Count > 0 && random.NextDouble() < SeedProfiles.DineInRate;

        var order = new Order
        {
            User = draft.User,
            CreatedAt = draft.CreatedAt,
            Status = draft.Status,
            OrderType = dineIn ? OrderType.DineIn : OrderType.Takeaway,
            TableID = dineIn ? Pick(tables, random).TableID : null,
            PaymentMethod = paymentMethod,
            PaymentStatus = isPaid ? PaymentStatus.Paid : PaymentStatus.Unpaid,
            PaidAt = isPaid ? Min(draft.CreatedAt.AddMinutes(random.Next(1, 11)), now) : null,

            // Bảng 3, dòng 12: seed không dùng voucher, nên FinalAmount = TotalAmount (CK_Orders_Amounts).
            DiscountAmount = 0m,
            TotalAmount = totalAmount,
            FinalAmount = totalAmount,
        };

        foreach (var detail in details)
        {
            order.OrderDetails.Add(detail);
        }

        return order;
    }

    /// <summary>
    /// Khoảng 25% dòng của đơn Completed có feedback của chính người đặt. Ca kiểm thử riêng:
    /// TS03 nhiều lượt ~4.3 sao, TS06 ~3.2 sao, món mới TT06 đúng 2 lượt 5 sao.
    /// </summary>
    private static List<Feedback> CreateFeedbacks(
        IReadOnlyList<OrderDraft> drafts,
        IReadOnlyList<Order> orders,
        Random random,
        DateTime now)
    {
        var feedbacks = new List<Feedback>();
        var lowReviewCount = 0;

        for (var i = 0; i < drafts.Count; i++)
        {
            var draft = drafts[i];
            if (draft.Status != OrderStatus.Completed)
            {
                continue;
            }

            foreach (var line in draft.Lines)
            {
                int rating;
                if (line.Item.Code == SeedProfiles.LowReviewNewProductCode)
                {
                    if (lowReviewCount >= SeedProfiles.LowReviewNewProductFeedbackCount)
                    {
                        continue;
                    }

                    lowReviewCount++;
                    rating = DbConstraints.RatingMax;
                }
                else
                {
                    var rule = SeedProfiles.RatingOverrides.FirstOrDefault(r => r.ProductCode == line.Item.Code);
                    if (random.NextDouble() >= (rule?.FeedbackRate ?? SeedProfiles.FeedbackRate))
                    {
                        continue;
                    }

                    rating = PickWeighted(rule?.Ratings ?? SeedProfiles.DefaultRatings, random);
                }

                feedbacks.Add(new Feedback
                {
                    User = draft.User,
                    Order = orders[i],
                    Product = line.Product,
                    Rating = rating,
                    Comment = random.NextDouble() < CommentlessRate ? null : Pick(CommentsByRating[rating], random),
                    CreatedAt = Min(draft.CreatedAt.AddHours(random.Next(1, 49)), now),
                });
            }
        }

        return feedbacks;
    }

    private static decimal SizeSurcharge(DrinkSize size)
    {
        return size switch
        {
            DrinkSize.M => SeedCatalog.SizeMSurcharge,
            DrinkSize.L => SeedCatalog.SizeLSurcharge,
            _ => 0m,
        };
    }

    /// <summary>Chọn một thời điểm trong khoảng, rồi đặt giờ theo phân phối giờ cao điểm (giờ Việt Nam).</summary>
    private static DateTime PickOrderTime(DateTime start, DateTime end, Random random)
    {
        var instant = start.AddTicks((long)(random.NextDouble() * (end - start).Ticks));
        var local = instant + VietnamOffset;
        var hour = PickWeighted(SeedProfiles.LocalOrderHours, random);
        var peak = new DateTime(local.Year, local.Month, local.Day, hour, random.Next(60), random.Next(60), DateTimeKind.Utc)
            - VietnamOffset;

        return peak >= start && peak <= end ? peak : instant;
    }

    private static T PickWeighted<T>(IReadOnlyList<Weighted<T>> options, Random random)
    {
        var total = options.Sum(o => o.Weight);
        var roll = random.NextDouble() * total;
        foreach (var option in options)
        {
            roll -= option.Weight;
            if (roll < 0)
            {
                return option.Value;
            }
        }

        return options.Last(o => o.Weight > 0).Value;
    }

    private static T PickEnum<T>(Random random)
        where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return values[random.Next(values.Length)];
    }

    private static T Pick<T>(IReadOnlyList<T> items, Random random)
    {
        return items[random.Next(items.Count)];
    }

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    private sealed record SeedUser(User Entity, UserProfile Profile);

    private sealed class OrderDraft
    {
        public OrderDraft(User user, UserProfile profile, DateTime createdAt, OrderStatus status)
        {
            User = user;
            Profile = profile;
            CreatedAt = createdAt;
            Status = status;
        }

        public User User { get; }
        public UserProfile Profile { get; }
        public DateTime CreatedAt { get; }
        public OrderStatus Status { get; }
        public List<OrderLine> Lines { get; } = new();
    }

    private sealed class OrderLine
    {
        public OrderLine(CatalogProduct item, Product product)
        {
            Item = item;
            Product = product;
        }

        public CatalogProduct Item { get; }
        public Product Product { get; }
        public int Quantity { get; set; }
        public DrinkSize Size { get; init; }
        public SugarLevel SugarLevel { get; init; }
        public IceLevel IceLevel { get; init; }
        public List<string> ToppingCodes { get; } = new();
    }

    /// <summary>Ánh xạ mã catalog sang entity đã lưu trong CSDL.</summary>
    private sealed class CatalogMap
    {
        private readonly IReadOnlyDictionary<string, Product> _products;
        private readonly IReadOnlyDictionary<string, Topping> _toppings;

        public CatalogMap(IReadOnlyDictionary<string, Product> products, IReadOnlyDictionary<string, Topping> toppings)
        {
            _products = products;
            _toppings = toppings;
        }

        public Product Product(string code) => _products[code];

        public Topping Topping(string code) => _toppings[code];

        public CatalogProduct Item(string code) => SeedCatalog.Products.First(p => p.Code == code);

        public CatalogTopping ToppingItem(string code) => SeedCatalog.Toppings.First(t => t.Code == code);
    }
}
