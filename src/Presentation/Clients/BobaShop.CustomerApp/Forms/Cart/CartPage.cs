using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Controls;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Forms.Cart;

/// <summary>
/// Trang Giỏ hàng, hiển thị bên trong CustomerMainForm khi bấm biểu tượng giỏ trên thanh đầu trang.
/// Bên trái là danh sách món đã thêm, bên phải là thẻ tóm tắt tổng tiền và nút Đặt hàng.
/// </summary>
[DesignerCategory("Code")]
public class CartPage : Panel
{
    private const int PagePadding = 32;
    private const int SummaryWidth = 320;
    private const int ColumnGap = 24;
    private const int RowGap = 12;
    private const int SummaryInset = 20;

    private readonly CartService _cart;
    private readonly List<CartLineRow> _rows = new();

    private readonly Label _lblTitle;
    private readonly Label _lblCount;

    private readonly Label _lblEmptyTitle;
    private readonly Label _lblEmptyHint;
    private readonly PrimaryButton _btnBrowse;

    private readonly Panel _pnlSummary;
    private readonly Label _lblSummaryTitle;
    private readonly Label _lblSubtotalCaption;
    private readonly Label _lblSubtotal;
    private readonly Panel _summaryDivider;
    private readonly Label _lblTotalCaption;
    private readonly Label _lblTotal;
    private readonly PrimaryButton _btnCheckout;
    private readonly LinkLabel _lnkContinue;
    private readonly LinkLabel _lnkClear;

    private bool _isLayingOut;

    public CartPage(CartService cart)
    {
        _cart = cart;

        AutoScroll = true;
        AutoScrollMargin = new Size(0, UiScale.Px(PagePadding));
        BackColor = AppColors.Background;

        _lblTitle = new Label
        {
            AutoSize = true,
            Font = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Text = "Giỏ hàng",
            UseMnemonic = false,
        };

        _lblCount = new Label
        {
            AutoSize = true,
            Font = AppFonts.Body,
            ForeColor = AppColors.TextSecondary,
            UseMnemonic = false,
        };

        _lblEmptyTitle = new Label
        {
            AutoSize = true,
            Font = AppFonts.Heading,
            ForeColor = AppColors.TextPrimary,
            Text = "Giỏ hàng đang trống",
            UseMnemonic = false,
        };

        _lblEmptyHint = new Label
        {
            AutoSize = true,
            Font = AppFonts.Body,
            ForeColor = AppColors.TextSecondary,
            Text = "Chọn món trong thực đơn, tùy chỉnh size, đường, đá, topping rồi thêm vào giỏ.",
            UseMnemonic = false,
        };

        _btnBrowse = new PrimaryButton
        {
            Text = "Xem thực đơn",
            Size = new Size(UiScale.Px(220), UiScale.Px(AppSpacing.ButtonHeight)),
        };

        _pnlSummary = new Panel { BackColor = AppColors.Surface };
        _pnlSummary.Paint += PnlSummary_Paint;

        _lblSummaryTitle = CreateSummaryLabel("Tóm tắt đơn hàng", AppFonts.Heading, AppColors.TextPrimary);
        _lblSubtotalCaption = CreateSummaryLabel(string.Empty, AppFonts.Body, AppColors.TextSecondary);
        _lblSubtotal = CreateSummaryLabel(string.Empty, AppFonts.Body, AppColors.TextPrimary);
        _summaryDivider = new Panel { BackColor = AppColors.Divider, Height = 1 };
        _lblTotalCaption = CreateSummaryLabel("Tổng cộng", AppFonts.BodyBold, AppColors.TextPrimary);
        _lblTotal = CreateSummaryLabel(string.Empty, AppFonts.Title, AppColors.Primary);
        _btnCheckout = new PrimaryButton { Text = "Đặt hàng" };
        _lnkContinue = CreateSummaryLink("Tiếp tục chọn món", AppColors.Primary);
        _lnkClear = CreateSummaryLink("Xóa tất cả", AppColors.TextSecondary);

        _pnlSummary.Controls.AddRange(new Control[]
        {
            _lblSummaryTitle, _lblSubtotalCaption, _lblSubtotal, _summaryDivider,
            _lblTotalCaption, _lblTotal, _btnCheckout, _lnkContinue, _lnkClear,
        });

        Controls.AddRange(new Control[] { _lblTitle, _lblCount, _lblEmptyTitle, _lblEmptyHint, _btnBrowse, _pnlSummary });

        _btnBrowse.Click += (_, _) => BrowseMenuRequested?.Invoke(this, EventArgs.Empty);
        _lnkContinue.LinkClicked += (_, _) => BrowseMenuRequested?.Invoke(this, EventArgs.Empty);
        _lnkClear.LinkClicked += (_, _) => _cart.Clear();
        _btnCheckout.Click += BtnCheckout_Click;
        _cart.Changed += (_, _) => SyncWithCart();
        Resize += (_, _) => LayoutContent();

        SyncWithCart();
    }

    /// <summary>Khách bấm "Xem thực đơn" hoặc "Tiếp tục chọn món". CustomerMainForm chuyển sang trang Thực đơn.</summary>
    public event EventHandler? BrowseMenuRequested;

    private static Label CreateSummaryLabel(string text, Font font, Color color)
    {
        return new Label
        {
            AutoSize = true,
            Font = font,
            ForeColor = color,
            BackColor = AppColors.Surface,
            Text = text,
            UseMnemonic = false,
        };
    }

    private static LinkLabel CreateSummaryLink(string text, Color color)
    {
        return new LinkLabel
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            Text = text,
            LinkColor = color,
            ActiveLinkColor = AppColors.PrimaryDark,
            VisitedLinkColor = color,
            LinkBehavior = LinkBehavior.HoverUnderline,
            BackColor = AppColors.Surface,
            UseMnemonic = false,
        };
    }

    /// <summary>Vẽ lại danh sách theo giỏ hiện tại. Nếu chỉ đổi số lượng thì cập nhật dòng có sẵn, không tạo lại.</summary>
    private void SyncWithCart()
    {
        var lines = _cart.Lines;
        var sameLines = _rows.Count == lines.Count
            && _rows.Select(row => row.Line).SequenceEqual(lines);

        if (sameLines)
        {
            foreach (var row in _rows)
            {
                row.RefreshValues();
            }
        }
        else
        {
            SuspendLayout();
            foreach (var row in _rows)
            {
                Controls.Remove(row);
                row.Dispose();
            }

            _rows.Clear();
            foreach (var line in lines)
            {
                var row = new CartLineRow(_cart, line);
                _rows.Add(row);
                Controls.Add(row);
            }

            ResumeLayout(false);
        }

        var isEmpty = lines.Count == 0;
        _lblEmptyTitle.Visible = isEmpty;
        _lblEmptyHint.Visible = isEmpty;
        _btnBrowse.Visible = isEmpty;
        _pnlSummary.Visible = !isEmpty;

        var quantity = _cart.TotalQuantity;
        _lblCount.Text = isEmpty ? string.Empty : $"{quantity} ly";
        _lblCount.Visible = !isEmpty;
        _lblSubtotalCaption.Text = $"Tạm tính ({quantity} ly)";
        _lblSubtotal.Text = Formatter.Currency(_cart.Subtotal);
        _lblTotal.Text = Formatter.Currency(_cart.Subtotal);

        LayoutContent();
    }

    private void BtnCheckout_Click(object? sender, EventArgs e)
    {
        // TODO: gọi API tạo đơn khi nhóm làm xong phần đặt hàng, thành công thì _cart.Clear().
        DialogHelper.ShowInfo(
            FindForm(),
            "Chức năng đặt hàng sẽ dùng được khi nhóm hoàn thành API đơn hàng. Giỏ hàng của bạn vẫn được giữ nguyên.");
    }

    private void LayoutContent()
    {
        if (_isLayingOut)
        {
            return;
        }

        _isLayingOut = true;
        try
        {
            var padding = UiScale.Px(PagePadding);
            var width = Math.Max(UiScale.Px(600), ClientSize.Width - (padding * 2));
            var origin = AutoScrollPosition;
            var x = padding + origin.X;
            var y = padding + origin.Y;

            _lblTitle.Location = new Point(x - UiScale.Px(6), y);
            if (_lblCount.Visible)
            {
                _lblCount.Location = new Point(
                    _lblTitle.Left + _lblTitle.PreferredWidth + UiScale.Px(12),
                    _lblTitle.Top + _lblTitle.PreferredHeight - _lblCount.PreferredHeight - UiScale.Px(8));
            }

            y += _lblTitle.PreferredHeight + UiScale.Px(16);

            if (_rows.Count == 0)
            {
                y += UiScale.Px(24);
                _lblEmptyTitle.Location = new Point(x, y);
                y += _lblEmptyTitle.PreferredHeight + UiScale.Px(8);
                _lblEmptyHint.Location = new Point(x, y);
                y += _lblEmptyHint.PreferredHeight + UiScale.Px(20);
                _btnBrowse.Location = new Point(x, y);
                return;
            }

            var summaryWidth = UiScale.Px(SummaryWidth);
            var listWidth = width - summaryWidth - UiScale.Px(ColumnGap);
            var rowY = y;
            foreach (var row in _rows)
            {
                row.SetBounds(x, rowY, listWidth, UiScale.Px(CartLineRow.RowHeight));
                rowY += row.Height + UiScale.Px(RowGap);
            }

            LayoutSummary(summaryWidth);
            _pnlSummary.Location = new Point(x + listWidth + UiScale.Px(ColumnGap), y);
        }
        finally
        {
            _isLayingOut = false;
        }
    }

    /// <summary>Xếp các dòng bên trong thẻ tóm tắt và đặt chiều cao thẻ vừa nội dung.</summary>
    private void LayoutSummary(int summaryWidth)
    {
        var inset = UiScale.Px(SummaryInset);
        var innerWidth = summaryWidth - (inset * 2);
        var y = inset;

        _lblSummaryTitle.Location = new Point(inset, y);
        y += _lblSummaryTitle.PreferredHeight + UiScale.Px(16);

        _lblSubtotalCaption.Location = new Point(inset, y);
        _lblSubtotal.Location = new Point(summaryWidth - inset - _lblSubtotal.PreferredWidth, y);
        y += _lblSubtotalCaption.PreferredHeight + UiScale.Px(16);

        _summaryDivider.SetBounds(inset, y, innerWidth, 1);
        y += UiScale.Px(16);

        var totalHeight = _lblTotal.PreferredHeight;
        _lblTotalCaption.Location = new Point(inset, y + ((totalHeight - _lblTotalCaption.PreferredHeight) / 2));
        _lblTotal.Location = new Point(summaryWidth - inset - _lblTotal.PreferredWidth, y);
        y += totalHeight + UiScale.Px(16);

        _btnCheckout.SetBounds(inset, y, innerWidth, UiScale.Px(AppSpacing.ButtonHeight));
        y += _btnCheckout.Height + UiScale.Px(12);

        _lnkContinue.Location = new Point(inset, y);
        _lnkClear.Location = new Point(summaryWidth - inset - _lnkClear.PreferredWidth, y);
        y += _lnkContinue.PreferredHeight + inset;

        _pnlSummary.Size = new Size(summaryWidth, y);
        _pnlSummary.Invalidate();
    }

    private void PnlSummary_Paint(object? sender, PaintEventArgs e)
    {
        using var border = new Pen(AppColors.Divider);
        e.Graphics.DrawRectangle(border, 0, 0, _pnlSummary.Width - 1, _pnlSummary.Height - 1);
    }
}
