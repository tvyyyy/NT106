using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.CustomerApp.Controls;

/// <summary>
/// Ô tài khoản trên thanh đầu trang: hình tròn mang chữ cái đầu của tên, bên cạnh là tên khách.
/// Bấm vào (hoặc nhấn Enter khi đang chọn) phát sự kiện Click để mở hồ sơ.
/// </summary>
[DesignerCategory("Code")]
public class UserChip : Control
{
    private const int AvatarSize = 36;
    private const int AvatarTextGap = 10;
    private const int RightPadding = 4;
    private const int MaxNameWidth = 200;

    private readonly string _displayName;
    private readonly string _initials;
    private bool _isHovered;

    public UserChip(string? fullName)
    {
        _displayName = string.IsNullOrWhiteSpace(fullName) ? "Tài khoản" : fullName.Trim();
        _initials = Formatter.Initials(fullName);

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

        // Bề rộng vừa khít hình tròn và tên, tên quá dài thì cắt bằng dấu ba chấm.
        var nameWidth = Math.Min(
            UiScale.Px(MaxNameWidth),
            TextRenderer.MeasureText(_displayName, AppFonts.BodyBold).Width);
        Size = new Size(
            UiScale.Px(AvatarSize) + UiScale.Px(AvatarTextGap) + nameWidth + UiScale.Px(RightPadding),
            UiScale.Px(AvatarSize) + UiScale.Px(4));
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

        var highlighted = _isHovered || Focused;
        var accent = highlighted ? AppColors.PrimaryDark : AppColors.Primary;

        // Hình tròn mang chữ cái đầu.
        var avatarSize = UiScale.Px(AvatarSize);
        var avatarBounds = new Rectangle(0, (Height - avatarSize) / 2, avatarSize - 1, avatarSize - 1);
        using var avatarBrush = new SolidBrush(accent);
        graphics.FillEllipse(avatarBrush, avatarBounds);

        using var initialsBrush = new SolidBrush(AppColors.OnPrimary);
        using var centered = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap,
        };
        graphics.DrawString(_initials, AppFonts.BodyBold, initialsBrush, avatarBounds, centered);

        // Tên khách, căn giữa theo chiều dọc với hình tròn.
        var textLeft = avatarSize + UiScale.Px(AvatarTextGap);
        var textBounds = new Rectangle(textLeft, 0, Math.Max(0, Width - textLeft), Height);
        TextRenderer.DrawText(
            graphics,
            _displayName,
            AppFonts.BodyBold,
            textBounds,
            accent,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
    }
}
