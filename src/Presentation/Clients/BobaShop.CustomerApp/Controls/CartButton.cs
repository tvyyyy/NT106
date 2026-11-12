using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp.Controls;

/// <summary>
/// Nút giỏ hàng trên thanh đầu trang: biểu tượng xe đẩy, có huy hiệu số món ở góc trên phải khi giỏ có hàng.
/// Bấm vào (hoặc nhấn Enter khi đang chọn) phát sự kiện Click để mở giỏ hàng.
/// Biểu tượng xe đẩy lấy từ bộ font biểu tượng có sẵn của Windows, không phải hình tự vẽ.
/// </summary>
[DesignerCategory("Code")]
public class CartButton : Control
{
    private const int ButtonSize = 44;
    private const int BadgeSize = 18;
    private const int MaxDisplayCount = 99;

    // Bộ font biểu tượng đi kèm Windows 10 và 11; E7BF là biểu tượng xe đẩy hàng.
    private const string IconFontFamily = "Segoe MDL2 Assets";
    private const string CartGlyph = "";
    private const float IconPointSize = 16F;
    private const float BadgePointSize = 7.5F;

    private readonly Font _iconFont;
    private readonly Font _badgeFont;
    private int _count;
    private bool _isHovered;

    public CartButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.Selectable,
            true);
        BackColor = AppColors.Surface;
        Cursor = Cursors.Hand;
        TabStop = true;

        _iconFont = new Font(IconFontFamily, IconPointSize, FontStyle.Regular);
        _badgeFont = new Font(AppFonts.BodyBold.FontFamily, BadgePointSize, FontStyle.Bold);
        Size = new Size(UiScale.Px(ButtonSize), UiScale.Px(ButtonSize));
    }

    /// <summary>Số món trong giỏ. Bằng 0 thì ẩn huy hiệu, quá 99 thì hiện "99+".</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Count
    {
        get => _count;
        set
        {
            var count = Math.Max(0, value);
            if (_count == count)
            {
                return;
            }

            _count = count;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode is Keys.Enter or Keys.Space)
        {
            e.Handled = true;
            OnClick(EventArgs.Empty);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.Clear(BackColor);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        using var centered = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap,
        };

        // Rê chuột hoặc đang được chọn thì hiện một nền tròn nhạt phía sau biểu tượng.
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        var highlighted = _isHovered || Focused;
        if (highlighted)
        {
            using var hoverBrush = new SolidBrush(AppColors.Background);
            graphics.FillEllipse(hoverBrush, bounds);
        }

        using var iconBrush = new SolidBrush(highlighted ? AppColors.PrimaryDark : AppColors.Primary);
        graphics.DrawString(CartGlyph, _iconFont, iconBrush, bounds, centered);

        if (_count <= 0)
        {
            return;
        }

        // Huy hiệu số món: tròn đỏ, chữ trắng, nằm ở góc trên phải của nút.
        var badgeSize = UiScale.Px(BadgeSize);
        var badge = new Rectangle(Width - badgeSize - 1, 0, badgeSize, badgeSize);
        using var badgeBrush = new SolidBrush(AppColors.Error);
        graphics.FillEllipse(badgeBrush, badge);

        var countText = _count > MaxDisplayCount ? $"{MaxDisplayCount}+" : _count.ToString();
        using var countBrush = new SolidBrush(AppColors.OnPrimary);
        graphics.DrawString(countText, _badgeFont, countBrush, badge, centered);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _iconFont.Dispose();
            _badgeFont.Dispose();
        }

        base.Dispose(disposing);
    }
}
