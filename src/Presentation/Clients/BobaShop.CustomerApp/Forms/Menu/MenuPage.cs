using System.ComponentModel;
using BobaShop.ClientNetworkLogic.Helpers;
using BobaShop.ClientNetworkLogic.Theme;
using BobaShop.CustomerApp.Controls;
using BobaShop.CustomerApp.Services;

namespace BobaShop.CustomerApp.Forms.Menu;

/// <summary>
/// Trang Thực đơn, hiển thị bên trong CustomerMainForm: toàn bộ đồ uống đang bán,
/// lọc theo danh mục và theo từ khóa gõ ở ô tìm kiếm trên thanh đầu trang.
/// </summary>
[DesignerCategory("Code")]
public class MenuPage : Panel
{
    private const int PagePadding = 32;

    private readonly IMenuService _menuService;
    private readonly List<Button> _categoryChips = new();

    private readonly Label _lblTitle;
    private readonly FlowLayoutPanel _flpCategories;
    private readonly Label _lblStatus;
    private readonly ProductGrid _grid;

    // Null nghĩa là đang chọn "Tất cả".
    private int? _selectedCategoryId;
    private string _searchQuery = string.Empty;
    private bool _isMenuLoaded;
    private bool _isLayingOut;

    public MenuPage(IMenuService menuService)
    {
        _menuService = menuService;

        AutoScroll = true;
        AutoScrollMargin = new Size(0, UiScale.Px(PagePadding));
        BackColor = AppColors.Background;

        _lblTitle = new Label
        {
            AutoSize = true,
            Font = AppFonts.Title,
            ForeColor = AppColors.TextPrimary,
            Text = "Thực đơn",
            UseMnemonic = false,
        };

        _flpCategories = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = AppColors.Background,
        };

        _lblStatus = new Label
        {
            AutoSize = true,
            Font = AppFonts.Body,
            ForeColor = AppColors.TextSecondary,
            UseMnemonic = false,
            Visible = false,
        };

        _grid = new ProductGrid();

        Controls.Add(_lblTitle);
        Controls.Add(_flpCategories);
        Controls.Add(_lblStatus);
        Controls.Add(_grid);

        Resize += (_, _) => LayoutContent();
        LayoutContent();
    }

    /// <summary>Tải danh mục và toàn bộ món.</summary>
    public async Task LoadAsync()
    {
        SetStatus("Đang tải thực đơn...");
        try
        {
            var categoriesTask = _menuService.GetCategoriesAsync();
            var productsTask = _menuService.GetProductsAsync();
            var categories = await categoriesTask;
            var products = await productsTask;
            if (IsDisposed)
            {
                return;
            }

            BuildCategoryChips(categories);
            _grid.SetProducts(products);
            _isMenuLoaded = true;
            ApplyFilter();
        }
        catch (Exception)
        {
            if (!IsDisposed)
            {
                SetStatus("Không tải được thực đơn. Vui lòng mở lại ứng dụng.");
            }
        }
    }

    /// <summary>Đặt từ khóa tìm kiếm (lấy từ ô tìm kiếm trên thanh đầu trang) và lọc lại.</summary>
    public void SetSearchQuery(string? text)
    {
        _searchQuery = SearchText.Normalize(text);
        ApplyFilter();
    }

    private void BuildCategoryChips(IReadOnlyList<MenuCategory> categories)
    {
        _flpCategories.SuspendLayout();
        _flpCategories.Controls.Clear();
        foreach (var chip in _categoryChips)
        {
            chip.Dispose();
        }

        _categoryChips.Clear();

        AddCategoryChip("Tất cả", null);
        foreach (var category in categories)
        {
            AddCategoryChip(category.CategoryName, category.CategoryId);
        }

        _flpCategories.ResumeLayout(true);
        UpdateChipStyles();
    }

    private void AddCategoryChip(string text, int? categoryId)
    {
        var chip = new Button
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, UiScale.Px(38)),
            Padding = new Padding(UiScale.Px(12), 0, UiScale.Px(12), 0),
            Margin = new Padding(0, 0, UiScale.Px(8), UiScale.Px(8)),
            FlatStyle = FlatStyle.Flat,
            Font = AppFonts.BodyBold,
            Text = text,
            Tag = categoryId,
            Cursor = Cursors.Hand,
            UseMnemonic = false,
            UseVisualStyleBackColor = false,
        };
        chip.FlatAppearance.BorderSize = 1;
        chip.Click += (_, _) =>
        {
            _selectedCategoryId = categoryId;
            UpdateChipStyles();
            ApplyFilter();
        };

        _categoryChips.Add(chip);
        _flpCategories.Controls.Add(chip);
    }

    private void UpdateChipStyles()
    {
        foreach (var chip in _categoryChips)
        {
            var selected = Equals(chip.Tag, _selectedCategoryId);
            chip.BackColor = selected ? AppColors.Primary : AppColors.Surface;
            chip.ForeColor = selected ? AppColors.OnPrimary : AppColors.TextPrimary;
            chip.FlatAppearance.BorderColor = selected ? AppColors.Primary : AppColors.InputBorder;
            chip.FlatAppearance.MouseOverBackColor = selected ? AppColors.PrimaryDark : AppColors.Divider;
            chip.FlatAppearance.MouseDownBackColor = selected ? AppColors.PrimaryDark : AppColors.Divider;
        }
    }

    /// <summary>Hiện các món khớp danh mục đang chọn và từ khóa tìm kiếm, ẩn các món còn lại.</summary>
    private void ApplyFilter()
    {
        if (!_isMenuLoaded)
        {
            // Gõ tìm kiếm trong lúc đang tải thì chưa có gì để lọc, tải xong sẽ lọc lại.
            return;
        }

        var visibleCount = _grid.ApplyFilter(product =>
            (_selectedCategoryId is null || product.CategoryId == _selectedCategoryId)
            && SearchText.Contains(product.ProductName, _searchQuery));

        SetStatus(visibleCount == 0 ? "Không tìm thấy món phù hợp." : null);
    }

    /// <summary>Đặt dòng trạng thái dưới hàng danh mục. Truyền null để ẩn.</summary>
    private void SetStatus(string? text)
    {
        _lblStatus.Text = text ?? string.Empty;
        _lblStatus.Visible = text is not null;
        LayoutContent();
    }

    /// <summary>Xếp tiêu đề, hàng danh mục và lưới món từ trên xuống theo bề rộng hiện tại.</summary>
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

            _lblTitle.Location = new Point(x - UiScale.Px(6), y);
            y += _lblTitle.PreferredHeight + UiScale.Px(12);

            var categoriesHeight = Math.Max(
                UiScale.Px(46),
                _flpCategories.GetPreferredSize(new Size(width, 0)).Height);
            _flpCategories.SetBounds(x, y, width, categoriesHeight);
            y += categoriesHeight + UiScale.Px(12);

            _lblStatus.Location = new Point(x, y);
            if (_lblStatus.Visible)
            {
                y += _lblStatus.PreferredHeight + UiScale.Px(12);
            }

            _grid.Location = new Point(x - _grid.HorizontalInset, y);
            _grid.LayoutForWidth(width);
        }
        finally
        {
            _isLayingOut = false;
        }
    }
}
