using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Controls;

/// <summary>
/// Lưới thẻ món dùng chung cho trang chủ và trang Thực đơn.
/// Số cột tự tính theo bề rộng để thẻ không hẹp hơn CardMinWidth.
/// </summary>
[DesignerCategory("Code")]
public class ProductGrid : FlowLayoutPanel
{
    private const int CardGap = 20;
    private const int CardMinWidth = 240;

    // Tên mục ảnh trong Properties/Resources.resx.
    private const string BrandCupsResourceName = "brand_cups";

    private readonly List<ProductCard> _cards = new();

    public ProductGrid()
    {
        FlowDirection = FlowDirection.LeftToRight;
        WrapContents = true;
        BackColor = AppColors.Background;
        Margin = Padding.Empty;
    }

    /// <summary>
    /// Mỗi thẻ có lề ngang nửa khoảng cách ở hai bên, nên lưới rộng hơn nội dung và phải đặt lùi sang trái
    /// đúng bằng giá trị này để mép thẻ thẳng hàng với các phần khác của trang.
    /// </summary>
    public int HorizontalInset => UiScale.Px(CardGap) / 2;

    public void SetProducts(IEnumerable<MenuProduct> products)
    {
        // TODO: khi API trả imageUrl thật thì tải ảnh theo từng món. Hiện mọi món đều chưa có ảnh
        // nên dùng luân phiên hai ảnh có sẵn trong Resources.
        var defaultImages = new[]
        {
            Properties.Resources.Cach_pha_tra_sua_hoa_nhai_ngon_la_mieng,
            Properties.Resources.ResourceManager.GetObject(BrandCupsResourceName) as Image,
        };

        SuspendLayout();
        Controls.Clear();
        foreach (var card in _cards)
        {
            card.Dispose();
        }

        _cards.Clear();

        var index = 0;
        foreach (var product in products)
        {
            var card = new ProductCard(product, defaultImages[index % defaultImages.Length]);
            _cards.Add(card);
            Controls.Add(card);
            index++;
        }

        ResumeLayout(false);
    }

    /// <summary>Hiện các món thỏa điều kiện, ẩn các món còn lại. Trả về số món đang hiện.</summary>
    public int ApplyFilter(Func<MenuProduct, bool> isVisible)
    {
        var visibleCount = 0;

        SuspendLayout();
        foreach (var card in _cards)
        {
            var visible = isVisible(card.Product);
            card.Visible = visible;
            if (visible)
            {
                visibleCount++;
            }
        }

        ResumeLayout(false);
        return visibleCount;
    }

    /// <summary>
    /// Tính lại kích thước thẻ và kích thước lưới cho bề rộng nội dung cho trước.
    /// Trả về chiều cao của lưới để trang xếp phần tiếp theo.
    /// </summary>
    public int LayoutForWidth(int contentWidth)
    {
        var gap = UiScale.Px(CardGap);
        var columns = Math.Max(1, (contentWidth + gap) / (UiScale.Px(CardMinWidth) + gap));
        var cardWidth = ((contentWidth + gap) / columns) - gap;
        var cardHeight = (int)(cardWidth * ProductCard.ImageRatio) + UiScale.Px(ProductCard.TextAreaHeight);
        var leftMargin = gap / 2;
        var visibleCount = 0;

        SuspendLayout();
        foreach (var card in _cards)
        {
            card.Size = new Size(cardWidth, cardHeight);
            card.Margin = new Padding(leftMargin, 0, gap - leftMargin, gap);
            if (card.Visible)
            {
                visibleCount++;
            }
        }

        var rows = (visibleCount + columns - 1) / columns;
        var height = rows * (cardHeight + gap);
        Size = new Size(contentWidth + gap, height);
        ResumeLayout(true);
        return height;
    }
}
