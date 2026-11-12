using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Controls;
using BobaShop.CustomerApp.Forms.Home;
using BobaShop.CustomerApp.Forms.Menu;
using BobaShop.CustomerApp.Forms.Profile;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Forms.Main;

/// <summary>
/// Khung chính của ứng dụng khách: thanh đầu trang (logo, điều hướng, ô tìm kiếm, đăng xuất)
/// và vùng nội dung hiển thị một trang tại một thời điểm (Trang chủ, Thực đơn).
/// Giao diện dựng bằng code, kích thước viết theo pixel ở 100% rồi nhân tỉ lệ màn hình qua UiScale.Px.
/// </summary>
[DesignerCategory("Code")]
public class CustomerMainForm : Form
{
    private const int HeaderHeight = 72;
    private const int PagePadding = 32;
    private const int LogoSize = 40;
    private const int NavGap = 28;
    private const int ActiveIndicatorHeight = 3;

    private readonly LoginResult _session;
    private readonly IProfileService _profileService;

    private readonly Panel _pnlHeader;
    private readonly Label _lblBrand;
    private readonly LinkLabel _lnkHome;
    private readonly LinkLabel _lnkMenu;
    private readonly SearchBox _txtSearch;
    private readonly UserChip _userChip;
    private readonly LinkLabel _lnkLogout;

    private readonly Panel _pnlPages;
    private readonly HomePage _homePage;
    private readonly MenuPage _menuPage;
    private readonly CartButton _btnCart;

    // Trang đang hiển thị và mục điều hướng ứng với nó (để vẽ vạch xanh bên dưới).
    private Control _activePage;
    private LinkLabel _activeNavLink;

    public CustomerMainForm(
        LoginResult session,
        IMenuService menuService,
        IPromotionService promotionService,
        IProfileService profileService)
    {
        _session = session;
        _profileService = profileService;

        SuspendLayout();

        Text = "BobaShop";
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(UiScale.Px(1200), UiScale.Px(720));
        MinimumSize = new Size(UiScale.Px(1080), UiScale.Px(600));
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = AppColors.Background;
        ForeColor = AppColors.TextPrimary;
        Font = AppFonts.Body;

        // Thanh đầu trang.
        _pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = UiScale.Px(HeaderHeight),
            BackColor = AppColors.Surface,
        };
        _pnlHeader.Paint += PnlHeader_Paint;

        var picLogo = new PictureBox
        {
            Image = Properties.Resources.download,
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(UiScale.Px(LogoSize), UiScale.Px(LogoSize)),
            Location = new Point(UiScale.Px(PagePadding), UiScale.Px((HeaderHeight - LogoSize) / 2)),
            TabStop = false,
        };

        _lblBrand = new Label
        {
            AutoSize = true,
            Font = AppFonts.Brand,
            ForeColor = AppColors.Primary,
            Text = "BobaShop",
            UseMnemonic = false,
        };

        _lnkHome = CreateNavLink("Trang chủ");
        _lnkMenu = CreateNavLink("Thực đơn");

        _txtSearch = new SearchBox
        {
            Size = new Size(UiScale.Px(220), UiScale.Px(40)),
            PlaceholderText = "Tìm đồ uống",
        };

        // Ô tài khoản: hình tròn chữ cái đầu và tên khách, bấm vào để mở hồ sơ.
        _userChip = new UserChip(_session.FullName);

        _lnkLogout = CreateNavLink("Đăng xuất");

        _pnlHeader.Controls.Add(picLogo);
        _pnlHeader.Controls.Add(_lblBrand);
        _pnlHeader.Controls.Add(_lnkHome);
        _pnlHeader.Controls.Add(_lnkMenu);
        _pnlHeader.Controls.Add(_txtSearch);
        _btnCart = new CartButton();

        _pnlHeader.Controls.Add(_btnCart);
        _pnlHeader.Controls.Add(_userChip);
        _pnlHeader.Controls.Add(_lnkLogout);

        // Vùng nội dung: mỗi trang phủ kín vùng này, chỉ một trang hiện tại một thời điểm.
        _pnlPages = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.Background,
        };

        _homePage = new HomePage(menuService, promotionService) { Dock = DockStyle.Fill };
        _menuPage = new MenuPage(menuService) { Dock = DockStyle.Fill, Visible = false };
        _pnlPages.Controls.Add(_homePage);
        _pnlPages.Controls.Add(_menuPage);

        _activePage = _homePage;
        _activeNavLink = _lnkHome;

        // Thêm panel Fill trước, panel Top sau để thanh đầu trang được dock trước.
        Controls.Add(_pnlPages);
        Controls.Add(_pnlHeader);

        ResumeLayout(false);

        _pnlHeader.Resize += (_, _) => LayoutHeader();
        _lnkHome.LinkClicked += (_, _) => ShowPage(_homePage, _lnkHome);
        _lnkMenu.LinkClicked += (_, _) => ShowPage(_menuPage, _lnkMenu);
        _btnCart.Click += BtnCart_Click;
        _lnkLogout.LinkClicked += LnkLogout_LinkClicked;
        _userChip.Click += UserChip_Click;
        _homePage.ViewAllRequested += (_, _) => ShowPage(_menuPage, _lnkMenu);
        _txtSearch.TextChanged += TxtSearch_TextChanged;
        Shown += CustomerMainForm_Shown;

        LayoutHeader();
    }

    /// <summary>True nếu người dùng bấm Đăng xuất, để Program.cs mở lại màn đăng nhập.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool LoggedOut { get; private set; }

    private static LinkLabel CreateNavLink(string text)
    {
        return new LinkLabel
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            Text = text,
            LinkColor = AppColors.Primary,
            ActiveLinkColor = AppColors.PrimaryDark,
            VisitedLinkColor = AppColors.Primary,
            LinkBehavior = LinkBehavior.HoverUnderline,
            TabStop = true,
            UseMnemonic = false,
        };
    }

    private async void CustomerMainForm_Shown(object? sender, EventArgs e)
    {
        // Hai trang tự bắt lỗi tải dữ liệu của mình và báo ngay trên trang.
        var homeTask = _homePage.LoadAsync();
        var menuTask = _menuPage.LoadAsync();
        await homeTask;
        await menuTask;
    }

    private void ShowPage(Control page, LinkLabel navLink)
    {
        if (ReferenceEquals(_activePage, page))
        {
            return;
        }

        page.Visible = true;
        _activePage.Visible = false;
        _activePage = page;
        _activeNavLink = navLink;
        _pnlHeader.Invalidate();
    }

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        // Tìm kiếm áp dụng cho toàn bộ thực đơn, nên gõ từ khóa ở trang khác thì chuyển sang Thực đơn.
        _menuPage.SetSearchQuery(_txtSearch.Text);
        if (!string.IsNullOrWhiteSpace(_txtSearch.Text))
        {
            ShowPage(_menuPage, _lnkMenu);
        }
    }

    private void LayoutHeader()
    {
        var height = _pnlHeader.ClientSize.Height;
        var padding = UiScale.Px(PagePadding);

        // Bên trái: logo, tên quán, điều hướng.
        var x = padding + UiScale.Px(LogoSize) + UiScale.Px(12);
        _lblBrand.Location = new Point(x, (height - _lblBrand.PreferredHeight) / 2);
        x += _lblBrand.PreferredWidth + UiScale.Px(32);
        foreach (var link in new[] { _lnkHome, _lnkMenu })
        {
            link.Location = new Point(x, (height - link.PreferredHeight) / 2);
            x += link.PreferredWidth + UiScale.Px(NavGap);
        }

        // Bên phải, xếp từ mép phải vào: đăng xuất, ô tài khoản, nút giỏ hàng, ô tìm kiếm.
        var right = _pnlHeader.ClientSize.Width - padding;
        right -= _lnkLogout.PreferredWidth;
        _lnkLogout.Location = new Point(right, (height - _lnkLogout.PreferredHeight) / 2);
        right -= UiScale.Px(20) + _userChip.Width;
        _userChip.Location = new Point(right, (height - _userChip.Height) / 2);
        right -= UiScale.Px(12) + _btnCart.Width;
        _btnCart.Location = new Point(right, (height - _btnCart.Height) / 2);
        right -= UiScale.Px(16) + _txtSearch.Width;
        _txtSearch.Location = new Point(right, (height - _txtSearch.Height) / 2);

        // Vạch xanh bám theo vị trí mục điều hướng nên phải vẽ lại sau khi xếp.
        _pnlHeader.Invalidate();
    }

    private void PnlHeader_Paint(object? sender, PaintEventArgs e)
    {
        // Đường kẻ mảnh ngăn thanh đầu trang với nội dung.
        using var pen = new Pen(AppColors.Divider);
        var y = _pnlHeader.ClientSize.Height - 1;
        e.Graphics.DrawLine(pen, 0, y, _pnlHeader.ClientSize.Width, y);

        // Vạch xanh dưới mục điều hướng đang mở, để người dùng biết mình đang ở trang nào.
        var indicatorHeight = UiScale.Px(ActiveIndicatorHeight);
        using var indicator = new SolidBrush(AppColors.Primary);
        e.Graphics.FillRectangle(
            indicator,
            _activeNavLink.Left,
            _pnlHeader.ClientSize.Height - indicatorHeight,
            _activeNavLink.Width,
            indicatorHeight);
    }

    private void BtnCart_Click(object? sender, EventArgs e)
    {
        // Màn giỏ hàng hiện có của Ngân.
        using var cartForm = new GH();
        cartForm.ShowDialog(this);
    }

    private void UserChip_Click(object? sender, EventArgs e)
    {
        using var profileForm = new ProfileForm(_profileService, _session.FullName);
        profileForm.ShowDialog(this);
    }

    private void LnkLogout_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        // TODO: xóa phiên trong SessionManager của Ngân khi có.
        LoggedOut = true;
        Close();
    }
}
