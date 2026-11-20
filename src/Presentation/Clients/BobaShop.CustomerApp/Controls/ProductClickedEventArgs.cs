using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Controls;

/// <summary>Thông tin món khách vừa bấm: món và ảnh đang hiện trên thẻ.</summary>
public sealed class ProductClickedEventArgs : EventArgs
{
    public ProductClickedEventArgs(MenuProduct product, Image? image)
    {
        Product = product;
        Image = image;
    }

    public MenuProduct Product { get; }

    public Image? Image { get; }
}
