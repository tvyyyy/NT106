using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Controls;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Controls;
using BobaShop.CustomerApp.Services;
using BobaShop.Domain.Enums;

namespace BobaShop.CustomerApp.Forms.Menu;

/// <summary>
/// Màn chi tiết một món, hiển thị bên trong CustomerMainForm khi khách bấm vào thẻ món.
/// Ảnh lớn bên trái; bên phải chọn size, mức đường, mức đá, topping, số lượng,
/// tổng tiền tự cập nhật và nút Thêm vào giỏ hàng.
/// </summary>
[DesignerCategory("Code")]
public class ProductDetailPage : Panel
{
    private const int PagePadding = 32;
    private const int ColumnGap = 40;
    private const int ImageMaxSize = 440;
    private const int SectionGap = 20;
    private const int LabelGap = 8;
    private const int AddButtonWidth = 300;

    private readonly IMenuService _menuService;
    private readonly CartService _cart;

    private readonly LinkLabel _lnkBack;
    private readonly CoverImageBox _image;
    private readonly Label _lblName;
    private readonly Label _lblBasePrice;
    private readonly Label _lblSize;
    private readonly ChoiceChips<DrinkSize> _sizeChips;
    private readonly Label _lblSugar;
    private readonly ChoiceChips<SugarLevel> _sugarChips;
    private readonly Label _lblIce;
    private readonly ChoiceChips<IceLevel> _iceChips;
    private readonly Label _lblTopping;
    private readonly Label _lblToppingStatus;
    private readonly ChoiceChips<MenuTopping> _toppingChips;
    private readonly Label _lblQuantity;
    private readonly QuantityStepper _quantity;
    private readonly Panel _divider;
    private readonly Label _lblTotalCaption;
    private readonly Label _lblTotal;
    private readonly PrimaryButton _btnAdd;
    private readonly Label _lblAdded;
    private readonly LinkLabel _lnkViewCart;

    private MenuProduct? _product;
    private Image? _productImage;
    private bool _isLayingOut;

    public ProductDetailPage(IMenuService menuService, CartService cart)
    {
        _menuService = menuService;
        _cart = cart;

        AutoScroll = true;
        AutoScrollMargin = new Size(0, UiScale.Px(PagePadding));
        BackColor = AppColors.Background;

        _lnkBack = new LinkLabel
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            Text = "‹  Quay lại",
            LinkColor = AppColors.Primary,
            ActiveLinkColor = AppColors.PrimaryDark,
            VisitedLinkColor = AppColors.Primary,
            LinkBehavior = LinkBehavior.HoverUnderline,
            UseMnemonic = false,
        };

        _image = new CoverImageBox();

        _lblName = new Label
        {
            AutoSize = true,
            Font = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            UseMnemonic = false,
        };

        _lblBasePrice = new Label
        {
            AutoSize = true,
            Font = AppFonts.Heading,
            ForeColor = AppColors.TextSecondary,
            UseMnemonic = false,
        };

        _lblSize = CreateSectionLabel("Kích cỡ");
        _sizeChips = new ChoiceChips<DrinkSize>(multiSelect: false);
        _sizeChips.SetChoices(DrinkOptions.Sizes.Select(choice =>
        {
            var surcharge = DrinkOptions.SizeSurcharge(choice.Value);
            var label = surcharge > 0 ? $"{choice.Label}  +{Formatter.Currency(surcharge)}" : choice.Label;
            return (choice.Value, label);
        }));

        _lblSugar = CreateSectionLabel("Mức đường");
        _sugarChips = new ChoiceChips<SugarLevel>(multiSelect: false);
        _sugarChips.SetChoices(DrinkOptions.SugarLevels.Select(choice => (choice.Value, choice.Label)));

        _lblIce = CreateSectionLabel("Mức đá");
        _iceChips = new ChoiceChips<IceLevel>(multiSelect: false);
        _iceChips.SetChoices(DrinkOptions.IceLevels.Select(choice => (choice.Value, choice.Label)));

        _lblTopping = CreateSectionLabel("Topping (chọn nhiều)");
        _lblToppingStatus = new Label
        {
            AutoSize = true,
            Font = AppFonts.Body,
            ForeColor = AppColors.TextSecondary,
            Text = "Đang tải topping...",
            UseMnemonic = false,
        };
        _toppingChips = new ChoiceChips<MenuTopping>(multiSelect: true);

        _lblQuantity = CreateSectionLabel("Số lượng");
        _quantity = new QuantityStepper { Maximum = CartService.MaxQuantity };

        _divider = new Panel { BackColor = AppColors.Divider, Height = 1 };

        _lblTotalCaption = new Label
        {
            AutoSize = true,
            Font = AppFonts.BodyLarge,
            ForeColor = AppColors.TextSecondary,
            Text = "Tổng cộng",
            UseMnemonic = false,
        };

        _lblTotal = new Label
        {
            AutoSize = true,
            Font = AppFonts.Title,
            ForeColor = AppColors.Primary,
            UseMnemonic = false,
        };

        _btnAdd = new PrimaryButton
        {
            Text = "Thêm vào giỏ hàng",
            Size = new Size(UiScale.Px(AddButtonWidth), UiScale.Px(AppSpacing.ButtonHeight)),
        };

        _lblAdded = new Label
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            ForeColor = AppColors.Primary,
            Text = "✓ Đã thêm vào giỏ hàng.",
            UseMnemonic = false,
            Visible = false,
        };

        _lnkViewCart = new LinkLabel
        {
            AutoSize = true,
            Font = AppFonts.BodyBold,
            Text = "Xem giỏ hàng",
            LinkColor = AppColors.Primary,
            ActiveLinkColor = AppColors.PrimaryDark,
            VisitedLinkColor = AppColors.Primary,
            UseMnemonic = false,
            Visible = false,
        };

        Controls.AddRange(new Control[]
        {
            _lnkBack, _image, _lblName, _lblBasePrice,
            _lblSize, _sizeChips, _lblSugar, _sugarChips, _lblIce, _iceChips,
            _lblTopping, _lblToppingStatus, _toppingChips, _lblQuantity, _quantity,
            _divider, _lblTotalCaption, _lblTotal, _btnAdd, _lblAdded, _lnkViewCart,
        });

        _lnkBack.LinkClicked += (_, _) => BackRequested?.Invoke(this, EventArgs.Empty);
        _lnkViewCart.LinkClicked += (_, _) => ViewCartRequested?.Invoke(this, EventArgs.Empty);
        _sizeChips.SelectionChanged += (_, _) => OnChoiceChanged();
        _sugarChips.SelectionChanged += (_, _) => OnChoiceChanged();
        _iceChips.SelectionChanged += (_, _) => OnChoiceChanged();
        _toppingChips.SelectionChanged += (_, _) => OnChoiceChanged();
        _quantity.ValueChanged += (_, _) => OnChoiceChanged();
        _btnAdd.Click += BtnAdd_Click;
        Resize += (_, _) => LayoutContent();

        ResetChoices();
        LayoutContent();
    }

    /// <summary>Khách bấm "Quay lại". CustomerMainForm trở về trang trước đó.</summary>
    public event EventHandler? BackRequested;

    /// <summary>Khách bấm "Xem giỏ hàng" sau khi thêm món.</summary>
    public event EventHandler? ViewCartRequested;

    /// <summary>Tải danh sách topping một lần khi mở ứng dụng. Lỗi thì báo ngay dưới mục Topping.</summary>
    public async Task LoadToppingsAsync()
    {
        try
        {
            var toppings = await _menuService.GetToppingsAsync();
            if (IsDisposed)
            {
                return;
            }

            _toppingChips.SetChoices(toppings.Select(topping =>
                (topping, $"{topping.ToppingName}  +{Formatter.Currency(topping.Price)}")));
            _lblToppingStatus.Text = toppings.Count == 0 ? "Hiện chưa có topping." : string.Empty;
            _lblToppingStatus.Visible = toppings.Count == 0;
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                _lblToppingStatus.Text = "Không tải được topping.";
                _lblToppingStatus.Visible = true;
            }
        }

        if (!IsDisposed)
        {
            UpdateTotal();
            LayoutContent();
        }
    }

    /// <summary>Hiện một món với các lựa chọn mặc định và cuộn về đầu trang.</summary>
    public void ShowProduct(MenuProduct product, Image? image)
    {
        _product = product;
        _productImage = image;
        _image.Picture = image;
        _lblName.Text = product.ProductName;
        _lblBasePrice.Text = $"Giá từ {Formatter.Currency(product.Price)}";

        ResetChoices();
        AutoScrollPosition = Point.Empty;
        LayoutContent();
    }

    private static Label CreateSectionLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Font = AppFonts.Heading,
            ForeColor = AppColors.TextPrimary,
            Text = text,
            UseMnemonic = false,
        };
    }

    private void ResetChoices()
    {
        _sizeChips.SelectValue(DrinkOptions.DefaultSize);
        _sugarChips.SelectValue(DrinkOptions.DefaultSugar);
        _iceChips.SelectValue(DrinkOptions.DefaultIce);
        _toppingChips.ClearSelection();
        _quantity.SetValueSilently(1);
        _lblAdded.Visible = false;
        _lnkViewCart.Visible = false;
        UpdateTotal();
    }

    private void OnChoiceChanged()
    {
        // Đổi lựa chọn sau khi đã thêm thì ẩn dòng "Đã thêm", tránh hiểu nhầm là món mới cũng đã vào giỏ.
        _lblAdded.Visible = false;
        _lnkViewCart.Visible = false;
        UpdateTotal();
    }

    private decimal CurrentUnitPrice()
    {
        return _product is null
            ? 0m
            : DrinkOptions.UnitPrice(_product.Price, _sizeChips.SelectedValue, _toppingChips.SelectedValues);
    }

    private void UpdateTotal()
    {
        _lblTotal.Text = Formatter.Currency(CurrentUnitPrice() * _quantity.Value);
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        if (_product is null)
        {
            return;
        }

        _cart.Add(new CartLine(
            _product,
            _productImage,
            _sizeChips.SelectedValue,
            _sugarChips.SelectedValue,
            _iceChips.SelectedValue,
            _toppingChips.SelectedValues,
            _quantity.Value));

        _lblAdded.Visible = true;
        _lnkViewCart.Visible = true;
        LayoutContent();
    }

    /// <summary>Xếp ảnh bên trái và cột lựa chọn bên phải theo bề rộng hiện tại.</summary>
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
            var left = padding + origin.X;
            var top = padding + origin.Y;

            _lnkBack.Location = new Point(left, top);
            top += _lnkBack.PreferredHeight + UiScale.Px(16);

            // Ảnh vuông chiếm khoảng 40% bề rộng, không quá ImageMaxSize.
            var imageSize = Math.Min(UiScale.Px(ImageMaxSize), (int)(width * 0.4));
            _image.SetBounds(left, top, imageSize, imageSize);

            var x = left + imageSize + UiScale.Px(ColumnGap);
            var columnWidth = Math.Max(UiScale.Px(280), width - imageSize - UiScale.Px(ColumnGap));
            var y = top;

            _lblName.MaximumSize = new Size(columnWidth, 0);
            _lblName.Location = new Point(x - UiScale.Px(4), y);
            y += _lblName.GetPreferredSize(new Size(columnWidth, 0)).Height + UiScale.Px(2);

            _lblBasePrice.Location = new Point(x, y);
            y += _lblBasePrice.PreferredHeight + UiScale.Px(SectionGap);

            y = PlaceSection(_lblSize, _sizeChips, x, y, columnWidth);
            y = PlaceSection(_lblSugar, _sugarChips, x, y, columnWidth);
            y = PlaceSection(_lblIce, _iceChips, x, y, columnWidth);

            _lblTopping.Location = new Point(x, y);
            y += _lblTopping.PreferredHeight + UiScale.Px(LabelGap);
            if (_lblToppingStatus.Visible)
            {
                _lblToppingStatus.Location = new Point(x, y);
                y += _lblToppingStatus.PreferredHeight + UiScale.Px(LabelGap);
            }

            var toppingHeight = _toppingChips.Controls.Count == 0
                ? 0
                : _toppingChips.GetPreferredSize(new Size(columnWidth, 0)).Height;
            _toppingChips.SetBounds(x, y, columnWidth, toppingHeight);
            y += toppingHeight + UiScale.Px(SectionGap - LabelGap);

            _lblQuantity.Location = new Point(x, y);
            y += _lblQuantity.PreferredHeight + UiScale.Px(LabelGap);
            _quantity.Location = new Point(x, y);
            y += _quantity.Height + UiScale.Px(SectionGap + 4);

            _divider.SetBounds(x, y, columnWidth, 1);
            y += UiScale.Px(16);

            // Hàng tổng tiền: chữ "Tổng cộng" bên trái, số tiền lớn bên phải cùng hàng.
            var totalHeight = _lblTotal.PreferredHeight;
            _lblTotalCaption.Location = new Point(x, y + ((totalHeight - _lblTotalCaption.PreferredHeight) / 2));
            _lblTotal.Location = new Point(x + columnWidth - _lblTotal.PreferredWidth, y);
            y += totalHeight + UiScale.Px(12);

            _btnAdd.Size = new Size(columnWidth, UiScale.Px(AppSpacing.ButtonHeight));
            _btnAdd.Location = new Point(x, y);
            y += _btnAdd.Height + UiScale.Px(12);

            _lblAdded.Location = new Point(x, y);
            _lnkViewCart.Location = new Point(x + _lblAdded.PreferredWidth + UiScale.Px(8), y);
        }
        finally
        {
            _isLayingOut = false;
        }
    }

    private static int PlaceSection(Label label, Control chips, int x, int y, int width)
    {
        label.Location = new Point(x, y);
        y += label.PreferredHeight + UiScale.Px(LabelGap);
        var height = chips.GetPreferredSize(new Size(width, 0)).Height;
        chips.SetBounds(x, y, width, height);
        return y + height + UiScale.Px(SectionGap - LabelGap);
    }
}
