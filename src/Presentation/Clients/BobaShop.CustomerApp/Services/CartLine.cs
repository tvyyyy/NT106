using BobaShop.Domain.Enums;

namespace BobaShop.CustomerApp.Services;

/// <summary>
/// Một dòng trong giỏ hàng: một món với đúng một bộ lựa chọn. Cùng món mà khác size, đường, đá
/// hoặc topping thì là dòng khác. Số lượng chỉ đổi qua CartService.
/// </summary>
public sealed class CartLine
{
    public CartLine(
        MenuProduct product,
        Image? image,
        DrinkSize size,
        SugarLevel sugar,
        IceLevel ice,
        IReadOnlyList<MenuTopping> toppings,
        int quantity)
    {
        Product = product;
        Image = image;
        Size = size;
        Sugar = sugar;
        Ice = ice;
        Toppings = toppings;
        Quantity = quantity;
    }

    public MenuProduct Product { get; }

    /// <summary>Ảnh đang hiện ở thẻ món, dùng lại cho ảnh nhỏ trong giỏ.</summary>
    public Image? Image { get; }

    public DrinkSize Size { get; }

    public SugarLevel Sugar { get; }

    public IceLevel Ice { get; }

    public IReadOnlyList<MenuTopping> Toppings { get; }

    public int Quantity { get; internal set; }

    public decimal UnitPrice => DrinkOptions.UnitPrice(Product.Price, Size, Toppings);

    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>True nếu dòng này cùng món và cùng toàn bộ lựa chọn, để gộp số lượng thay vì thêm dòng mới.</summary>
    public bool HasSameChoices(CartLine other)
    {
        return Product.ProductId == other.Product.ProductId
            && Size == other.Size
            && Sugar == other.Sugar
            && Ice == other.Ice
            && Toppings.Select(topping => topping.ToppingId).OrderBy(id => id)
                .SequenceEqual(other.Toppings.Select(topping => topping.ToppingId).OrderBy(id => id));
    }
}
