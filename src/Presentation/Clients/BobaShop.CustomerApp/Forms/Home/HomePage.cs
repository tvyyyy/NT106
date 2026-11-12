using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Controls;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Forms.Home;

/// <summary>
/// Trang chủ, hiển thị bên trong CustomerMainForm: banner ảnh tự chuyển, khu vực khuyến mãi
/// và một số món nổi bật. Toàn bộ thực đơn nằm ở trang Thực đơn.
/// </summary>
[DesignerCategory("Code")]
public class HomePage : Panel
{
    private const int PagePadding = 32;
    private const int BannerHeight = 240;
    private const int SectionGap = 28;
    private const int PromotionGap = 20;
    private const int PromotionMinWidth = 300;

    /// <summary>Số món nổi bật tối đa hiển thị ở trang chủ.</summary>
    private const int FeaturedLimit = 8;

    // Tên mục ảnh trong Properties/Resources.resx.
    private const string BrandCupsResourceName = "brand_cups";

    private readonly IMenuService _menuService;
    private readonly IPromotionService _promotionService;
    private readonly List<PromotionCard> _promotionCards = new();

    private readonly ImageSlideshow _banner;
    private readonly Label _lblPromotions;
    private readonly Label _lblPromotionStatus;
    private readonly Label _lblFeatured;
    private readonly LinkLabel _lnkViewAll;
    private readonly Label _lblFeaturedStatus;
    private readonly ProductGrid _grid;
    private readonly PageFooter _footer;

    private bool _isLayingOut;

    public HomePage(IMenuService menuService, IPromotionService promotionService)
    {
        _menuService = menuService;
        _promotionService = promotionService;

        // Chân trang nằm sát đáy vùng cuộn nên trang này không thêm lề dưới.
        AutoScroll = true;
        BackColor = AppColors.Background;

        _banner = new ImageSlideshow
        {
            BackColor = AppColors.BrandDark,
            ZoomAmount = 0.04F,
            ShowNavigation = true,
            TabStop = false,
        };
        _banner.SetImages(new[]
        {
            Properties.Resources.ResourceManager.GetObject(BrandCupsResourceName) as Image,
            Properties.Resources.download__1_,
            Properties.Resources.Cach_pha_tra_sua_hoa_nhai_ngon_la_mieng,
        });

        _lblPromotions = CreateSectionTitle("Khuyến mãi");
        _lblPromotionStatus = CreateStatusLabel();
        _lblFeatured = CreateSectionTitle("Món nổi bật");
        _lblFeaturedStatus = CreateStatusLabel();

        _lnkViewAll = new LinkLabel
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            Text = "Xem tất cả thực đơn",
            LinkColor = AppColors.Primary,
            ActiveLinkColor = AppColors.PrimaryDark,
            VisitedLinkColor = AppColors.Primary,
            LinkBehavior = LinkBehavior.HoverUnderline,
            TabStop = true,
            UseMnemonic = false,
        };
        _lnkViewAll.LinkClicked += (_, _) => ViewAllRequested?.Invoke(this, EventArgs.Empty);

        _grid = new ProductGrid();
        _footer = new PageFooter();

        Controls.Add(_banner);
        Controls.Add(_lblPromotions);
        Controls.Add(_lblPromotionStatus);
        Controls.Add(_lblFeatured);
        Controls.Add(_lnkViewAll);
        Controls.Add(_lblFeaturedStatus);
        Controls.Add(_grid);
        Controls.Add(_footer);

        Resize += (_, _) => LayoutContent();
        LayoutContent();
    }

    /// <summary>Người dùng bấm "Xem tất cả thực đơn". CustomerMainForm chuyển sang trang Thực đơn.</summary>
    public event EventHandler? ViewAllRequested;

    /// <summary>Tải khuyến mãi và món nổi bật. Lỗi phần nào thì báo ở phần đó, không ảnh hưởng phần còn lại.</summary>
    public async Task LoadAsync()
    {
        SetStatus(_lblPromotionStatus, "Đang tải khuyến mãi...");
        SetStatus(_lblFeaturedStatus, "Đang tải món nổi bật...");

        var promotionsTask = LoadPromotionsAsync();
        var featuredTask = LoadFeaturedAsync();
        await promotionsTask;
        await featuredTask;
    }

    private static Label CreateSectionTitle(string text)
    {
        return new Label
        {
            AutoSize = true,
            Font = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Text = text,
            UseMnemonic = false,
        };
    }

    private static Label CreateStatusLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = AppFonts.Body,
            ForeColor = AppColors.TextSecondary,
            UseMnemonic = false,
            Visible = false,
        };
    }

    private async Task LoadPromotionsAsync()
    {
        try
        {
            var promotions = await _promotionService.GetActivePromotionsAsync();
            if (IsDisposed)
            {
                return;
            }

            foreach (var card in _promotionCards)
            {
                Controls.Remove(card);
                card.Dispose();
            }

            _promotionCards.Clear();
            foreach (var promotion in promotions)
            {
                var card = new PromotionCard(promotion);
                _promotionCards.Add(card);
                Controls.Add(card);
            }

            SetStatus(_lblPromotionStatus, promotions.Count == 0 ? "Hiện chưa có khuyến mãi nào." : null);
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                SetStatus(_lblPromotionStatus, "Không tải được khuyến mãi.");
            }
        }
    }

    private async Task LoadFeaturedAsync()
    {
        try
        {
            var products = await _menuService.GetFeaturedProductsAsync();
            if (IsDisposed)
            {
                return;
            }

            _grid.SetProducts(products.Take(FeaturedLimit));
            SetStatus(_lblFeaturedStatus, products.Count == 0 ? "Hiện chưa có món nổi bật." : null);
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                SetStatus(_lblFeaturedStatus, "Không tải được món nổi bật.");
            }
        }
    }

    /// <summary>Đặt dòng trạng thái của một khu vực. Truyền null để ẩn.</summary>
    private void SetStatus(Label label, string? text)
    {
        label.Text = text ?? string.Empty;
        label.Visible = text is not null;
        LayoutContent();
    }

    /// <summary>Xếp các khu vực từ trên xuống theo bề rộng hiện tại của trang.</summary>
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
            var width = Math.Max(UiScale.Px(400), ClientSize.Width - (padding * 2));

            // Khi đang cuộn, vị trí control con tính theo điểm cuộn hiện tại.
            var origin = AutoScrollPosition;
            var x = padding + origin.X;
            var y = padding + origin.Y;
            var titleInset = UiScale.Px(6);

            _banner.SetBounds(x, y, width, UiScale.Px(BannerHeight));
            y += UiScale.Px(BannerHeight) + UiScale.Px(SectionGap);

            // Khuyến mãi.
            _lblPromotions.Location = new Point(x - titleInset, y);
            y += _lblPromotions.PreferredHeight + UiScale.Px(8);

            _lblPromotionStatus.Location = new Point(x, y);
            if (_lblPromotionStatus.Visible)
            {
                y += _lblPromotionStatus.PreferredHeight + UiScale.Px(12);
            }

            if (_promotionCards.Count > 0)
            {
                var gap = UiScale.Px(PromotionGap);
                var fitting = Math.Max(1, (width + gap) / (UiScale.Px(PromotionMinWidth) + gap));
                var columns = Math.Min(_promotionCards.Count, fitting);
                var cardWidth = ((width + gap) / columns) - gap;
                var cardHeight = UiScale.Px(PromotionCard.CardHeight);
                for (var i = 0; i < _promotionCards.Count; i++)
                {
                    var column = i % columns;
                    var row = i / columns;
                    _promotionCards[i].SetBounds(
                        x + (column * (cardWidth + gap)),
                        y + (row * (cardHeight + gap)),
                        cardWidth,
                        cardHeight);
                }

                var rows = (_promotionCards.Count + columns - 1) / columns;
                y += (rows * (cardHeight + gap)) - gap;
            }

            y += UiScale.Px(SectionGap);

            // Món nổi bật, liên kết "Xem tất cả" nằm cùng hàng ở mép phải.
            _lblFeatured.Location = new Point(x - titleInset, y);
            _lnkViewAll.Location = new Point(
                x + width - _lnkViewAll.PreferredWidth,
                y + _lblFeatured.PreferredHeight - _lnkViewAll.PreferredHeight - UiScale.Px(8));
            y += _lblFeatured.PreferredHeight + UiScale.Px(8);

            _lblFeaturedStatus.Location = new Point(x, y);
            if (_lblFeaturedStatus.Visible)
            {
                y += _lblFeaturedStatus.PreferredHeight + UiScale.Px(12);
            }

            _grid.Location = new Point(x - _grid.HorizontalInset, y);
            y += _grid.LayoutForWidth(width) + UiScale.Px(SectionGap);

            // Chân trang phủ hết bề ngang, không chừa lề hai bên như các khu vực phía trên.
            _footer.SetBounds(origin.X, y, ClientSize.Width, UiScale.Px(PageFooter.FooterHeight));
        }
        finally
        {
            _isLayingOut = false;
        }
    }
}
