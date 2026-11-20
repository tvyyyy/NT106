using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Controls;

/// <summary>
/// Một dòng trong trang giỏ hàng: ảnh nhỏ, tên món, các lựa chọn, ô số lượng, thành tiền và nút Xóa.
/// Đổi số lượng hoặc bấm Xóa thì gọi thẳng CartService; trang giỏ hàng nghe CartService.Changed để vẽ lại.
/// </summary>
[DesignerCategory("Code")]
public class CartLineRow : Control
{
    public const int RowHeight = 112;

    private const int Inset = 16;
    private const int ThumbSize = 80;
    private const int TotalWidth = 130;
    private const int ColumnGap = 20;

    private readonly CartService _cart;
    private readonly QuantityStepper _quantity;
    private readonly LinkLabel _lnkRemove;

    public CartLineRow(CartService cart, CartLine line)
    {
        _cart = cart;
        Line = line;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw,
            true);
        BackColor = AppColors.Surface;

        _quantity = new QuantityStepper { Maximum = CartService.MaxQuantity, BackColor = AppColors.Surface };
        _quantity.SetValueSilently(line.Quantity);

        _lnkRemove = new LinkLabel
        {
            AutoSize = true,
            Font = AppFonts.Caption,
            Text = "Xóa",
            LinkColor = AppColors.TextSecondary,
            ActiveLinkColor = AppColors.Error,
            VisitedLinkColor = AppColors.TextSecondary,
            LinkBehavior = LinkBehavior.HoverUnderline,
            BackColor = AppColors.Surface,
            UseMnemonic = false,
        };

        Controls.Add(_quantity);
        Controls.Add(_lnkRemove);

        _quantity.ValueChanged += (_, _) => _cart.SetQuantity(Line, _quantity.Value);
        // Xóa dòng làm trang giỏ hàng Dispose chính dòng này, nên dời việc xóa ra sau khi sự kiện bấm chạy xong.
        _lnkRemove.LinkClicked += (_, _) => BeginInvoke(new MethodInvoker(() => _cart.Remove(Line)));

        Height = UiScale.Px(RowHeight);
    }

    public CartLine Line { get; }

    /// <summary>Đồng bộ số lượng và thành tiền sau khi giỏ thay đổi.</summary>
    public void RefreshValues()
    {
        _quantity.SetValueSilently(Line.Quantity);
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        var inset = UiScale.Px(Inset);
        var stepperX = Width - inset - UiScale.Px(TotalWidth) - UiScale.Px(ColumnGap) - _quantity.Width;
        _quantity.Location = new Point(stepperX, (Height - _quantity.Height) / 2);
        _lnkRemove.Location = new Point(
            inset + UiScale.Px(ThumbSize) + inset,
            Height - inset - _lnkRemove.PreferredHeight);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.Clear(AppColors.Background);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        // Nền thẻ trắng bo góc, viền mảnh.
        var radius = UiScale.Px(AppSpacing.Radius12);
        var card = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = ImageDrawing.RoundedRectangle(card, radius))
        using (var fill = new SolidBrush(AppColors.Surface))
        using (var border = new Pen(AppColors.Divider))
        {
            graphics.FillPath(fill, path);
            graphics.DrawPath(border, path);
        }

        var inset = UiScale.Px(Inset);
        var thumbSize = UiScale.Px(ThumbSize);
        var thumb = new Rectangle(inset, (Height - thumbSize) / 2, thumbSize, thumbSize);
        ImageDrawing.DrawCover(graphics, Line.Image, thumb, UiScale.Px(8), AppColors.Divider);

        // Cột chữ giữa ảnh và ô số lượng.
        var textLeft = thumb.Right + inset;
        var textWidth = Math.Max(0, _quantity.Left - UiScale.Px(ColumnGap) - textLeft);
        var y = (float)inset;

        using var nameBrush = new SolidBrush(AppColors.TextPrimary);
        using var detailBrush = new SolidBrush(AppColors.TextSecondary);
        using var priceBrush = new SolidBrush(AppColors.Primary);
        using var singleLine = new StringFormat
        {
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap,
        };

        var nameHeight = AppFonts.BodyBold.GetHeight(graphics);
        graphics.DrawString(Line.Product.ProductName, AppFonts.BodyBold, nameBrush, new RectangleF(textLeft, y, textWidth, nameHeight), singleLine);
        y += nameHeight + UiScale.Px(2);

        var captionHeight = AppFonts.Caption.GetHeight(graphics);
        var choices = DrinkOptions.Describe(Line.Size, Line.Sugar, Line.Ice);
        graphics.DrawString(choices, AppFonts.Caption, detailBrush, new RectangleF(textLeft, y, textWidth, captionHeight), singleLine);
        y += captionHeight;

        if (Line.Toppings.Count > 0)
        {
            var toppings = "Topping: " + string.Join(", ", Line.Toppings.Select(topping => topping.ToppingName));
            graphics.DrawString(toppings, AppFonts.Caption, detailBrush, new RectangleF(textLeft, y, textWidth, captionHeight), singleLine);
        }

        // Thành tiền căn phải, đơn giá nhỏ ngay dưới.
        var totalWidth = UiScale.Px(TotalWidth);
        var totalLeft = Width - inset - totalWidth;
        using var rightAligned = new StringFormat
        {
            Alignment = StringAlignment.Far,
            FormatFlags = StringFormatFlags.NoWrap,
        };
        var totalHeight = AppFonts.Heading.GetHeight(graphics);
        var totalTop = (Height - totalHeight - captionHeight) / 2F;
        graphics.DrawString(Formatter.Currency(Line.LineTotal), AppFonts.Heading, priceBrush, new RectangleF(totalLeft, totalTop, totalWidth, totalHeight), rightAligned);
        graphics.DrawString($"{Formatter.Currency(Line.UnitPrice)} / ly", AppFonts.Caption, detailBrush, new RectangleF(totalLeft, totalTop + totalHeight, totalWidth, captionHeight), rightAligned);
    }
}
