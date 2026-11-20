using BobaShop.Domain.Enums;

namespace BobaShop.CustomerApp.Services;

/// <summary>Một lựa chọn hiển thị trên màn chi tiết món: giá trị và chữ hiện trên nút.</summary>
public sealed record DrinkChoice<T>(T Value, string Label);

/// <summary>
/// Các lựa chọn size, mức đường, mức đá và cách tính giá một ly. Dùng enum của Domain
/// để sau này gửi đơn lên API không phải đổi kiểu.
/// </summary>
public static class DrinkOptions
{
    /// <summary>Phụ thu theo size, khớp SeedCatalog.SizeMSurcharge và SizeLSurcharge phía server.</summary>
    public const decimal SizeMSurcharge = 5000m;
    public const decimal SizeLSurcharge = 10000m;

    public const DrinkSize DefaultSize = DrinkSize.S;
    public const SugarLevel DefaultSugar = SugarLevel.Percent100;
    public const IceLevel DefaultIce = IceLevel.Normal;

    public static readonly IReadOnlyList<DrinkChoice<DrinkSize>> Sizes = new[]
    {
        new DrinkChoice<DrinkSize>(DrinkSize.S, "Nhỏ (S)"),
        new DrinkChoice<DrinkSize>(DrinkSize.M, "Vừa (M)"),
        new DrinkChoice<DrinkSize>(DrinkSize.L, "Lớn (L)"),
    };

    public static readonly IReadOnlyList<DrinkChoice<SugarLevel>> SugarLevels = new[]
    {
        new DrinkChoice<SugarLevel>(SugarLevel.Percent0, "0%"),
        new DrinkChoice<SugarLevel>(SugarLevel.Percent30, "30%"),
        new DrinkChoice<SugarLevel>(SugarLevel.Percent50, "50%"),
        new DrinkChoice<SugarLevel>(SugarLevel.Percent70, "70%"),
        new DrinkChoice<SugarLevel>(SugarLevel.Percent100, "100%"),
    };

    public static readonly IReadOnlyList<DrinkChoice<IceLevel>> IceLevels = new[]
    {
        new DrinkChoice<IceLevel>(IceLevel.NoIce, "Không đá"),
        new DrinkChoice<IceLevel>(IceLevel.Less, "Ít đá"),
        new DrinkChoice<IceLevel>(IceLevel.Normal, "Vừa"),
        new DrinkChoice<IceLevel>(IceLevel.Extra, "Nhiều đá"),
    };

    public static decimal SizeSurcharge(DrinkSize size)
    {
        return size switch
        {
            DrinkSize.M => SizeMSurcharge,
            DrinkSize.L => SizeLSurcharge,
            _ => 0m,
        };
    }

    /// <summary>Giá một ly: giá món, cộng phụ thu size, cộng các topping đã chọn.</summary>
    public static decimal UnitPrice(decimal basePrice, DrinkSize size, IEnumerable<MenuTopping> toppings)
    {
        return basePrice + SizeSurcharge(size) + toppings.Sum(topping => topping.Price);
    }

    public static string SizeLabel(DrinkSize size)
    {
        return Sizes.First(choice => choice.Value == size).Label;
    }

    /// <summary>Mô tả ngắn các lựa chọn của một ly, ví dụ "Size M · 50% đường · Ít đá".</summary>
    public static string Describe(DrinkSize size, SugarLevel sugar, IceLevel ice)
    {
        var sugarLabel = SugarLevels.First(choice => choice.Value == sugar).Label;
        var iceLabel = IceLevels.First(choice => choice.Value == ice).Label;
        var iceText = ice == IceLevel.Normal ? "Đá vừa" : iceLabel;
        return $"Size {size} · {sugarLabel} đường · {iceText}";
    }
}
