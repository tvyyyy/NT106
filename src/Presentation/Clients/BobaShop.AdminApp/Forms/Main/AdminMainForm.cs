using System.ComponentModel;
using BobaShop.AdminApp.Services;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;

namespace BobaShop.AdminApp.Forms.Main;

/// <summary>
/// Khung chính của AdminApp sau khi đăng nhập. Trong phạm vi giữa kỳ mới có thanh đầu trang
/// (tên quản lý, đăng xuất); các màn quản lý thực đơn và voucher sẽ gắn vào vùng nội dung sau.
/// </summary>
[DesignerCategory("Code")]
public class AdminMainForm : Form
{
    private const int HeaderHeight = 72;
    private const int PagePadding = 32;

    private readonly Panel _pnlHeader;
    private readonly Label _lblBrand;
    private readonly Label _lblAccount;
    private readonly LinkLabel _lnkLogout;

    public AdminMainForm(AdminLoginResult session)
    {
        SuspendLayout();

        Text = "BobaShop Quản trị";
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(UiScale.Px(1000), UiScale.Px(640));
        MinimumSize = new Size(UiScale.Px(800), UiScale.Px(480));
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = AppFonts.Body;

        _pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = UiScale.Px(HeaderHeight),
            BackColor = AppColors.Surface,
        };
        _pnlHeader.Paint += PnlHeader_Paint;

        _lblBrand = new Label
        {
            AutoSize = true,
            Font = AppFonts.Brand,
            ForeColor = AppColors.Primary,
            Text = "BobaShop Quản trị",
            UseMnemonic = false,
        };

        var displayName = string.IsNullOrWhiteSpace(session.FullName) ? session.Email : session.FullName;
        _lblAccount = new Label
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            ForeColor = AppColors.TextPrimary,
            Text = displayName ?? string.Empty,
            UseMnemonic = false,
        };

        _lnkLogout = new LinkLabel
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            Text = "Đăng xuất",
            LinkColor = AppColors.Primary,
            ActiveLinkColor = AppColors.PrimaryDark,
            VisitedLinkColor = AppColors.Primary,
            LinkBehavior = LinkBehavior.HoverUnderline,
            TabStop = true,
            UseMnemonic = false,
        };

        _pnlHeader.Controls.Add(_lblBrand);
        _pnlHeader.Controls.Add(_lblAccount);
        _pnlHeader.Controls.Add(_lnkLogout);

        var lblPlaceholder = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = AppColors.TextSecondary,
            Text = "Bạn đã đăng nhập với quyền quản lý.\nCác màn quản lý thực đơn và voucher sẽ được bổ sung ở giai đoạn sau.",
            TextAlign = ContentAlignment.MiddleCenter,
            UseMnemonic = false,
        };

        // Thêm control Fill trước, panel Top sau để thanh đầu trang được dock trước.
        Controls.Add(lblPlaceholder);
        Controls.Add(_pnlHeader);

        ResumeLayout(false);

        _pnlHeader.Resize += (_, _) => LayoutHeader();
        _lnkLogout.LinkClicked += LnkLogout_LinkClicked;
        LayoutHeader();
    }

    /// <summary>True nếu người dùng bấm Đăng xuất, để Program.cs mở lại màn đăng nhập.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool LoggedOut { get; private set; }

    private void LayoutHeader()
    {
        var height = _pnlHeader.ClientSize.Height;
        var padding = UiScale.Px(PagePadding);

        _lblBrand.Location = new Point(padding, (height - _lblBrand.PreferredHeight) / 2);

        // Bên phải, xếp từ mép phải vào: đăng xuất, tên quản lý.
        var right = _pnlHeader.ClientSize.Width - padding - _lnkLogout.PreferredWidth;
        _lnkLogout.Location = new Point(right, (height - _lnkLogout.PreferredHeight) / 2);
        right -= UiScale.Px(20) + _lblAccount.PreferredWidth;
        _lblAccount.Location = new Point(right, (height - _lblAccount.PreferredHeight) / 2);
    }

    private void PnlHeader_Paint(object? sender, PaintEventArgs e)
    {
        // Đường kẻ mảnh ngăn thanh đầu trang với nội dung.
        using var pen = new Pen(AppColors.Divider);
        var y = _pnlHeader.ClientSize.Height - 1;
        e.Graphics.DrawLine(pen, 0, y, _pnlHeader.ClientSize.Width, y);
    }

    private void LnkLogout_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        LoggedOut = true;
        Close();
    }
}
